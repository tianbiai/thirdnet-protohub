/**
 * API 请求适配器（Web 端 fetch 实现）
 *
 * 认证策略：
 * - 已登录（有 JWT token）：使用 Bearer token 认证
 * - 未登录（如获取 token 前的请求）：使用 HMAC-SHA512 签名的 Basic 认证
 *
 * 自动处理后端 snake_case 与前端 camelCase 的 key 转换
 */

import type { RequestConfig } from '@/api/types/common'
import { transformKeysToCamel, transformKeysToSnake } from '@/utils/transform'
import { generateBasicAuth, getTimestamp } from '@/utils/signature'
import { getToken, clearToken } from '@/utils/token'
import { API_BASE_URL } from '@/config'

/** 延迟导入路由和 store 避免循环依赖 */
let _router: typeof import('@/router')['default'] | null = null
let _useUserStore: typeof import('@/stores/user')['useUserStore'] | null = null
let _useMenuStore: typeof import('@/stores/menu')['useMenuStore'] | null = null

async function getRouter() {
  if (!_router) {
    const mod = await import('@/router')
    _router = mod.default
  }
  return _router
}

async function getUserStore() {
  if (!_useUserStore) {
    const mod = await import('@/stores/user')
    _useUserStore = mod.useUserStore
  }
  return _useUserStore()
}

async function getMenuStore() {
  if (!_useMenuStore) {
    const mod = await import('@/stores/menu')
    _useMenuStore = mod.useMenuStore
  }
  return _useMenuStore()
}

/**
 * 处理 fetch 响应
 * @param response - fetch Response 对象
 * @param rawResponse - 为 true 时不转换 key（用于 token 等保持 snake_case 的接口）
 * @param skipAuthRedirect - 为 true 时跳过 401 自动跳转
 */
async function processResponse<T>(
  response: Response,
  rawResponse: boolean = false,
  skipAuthRedirect: boolean = false
): Promise<T> {
  if (!response.ok) {
    let errorMessage = `请求失败: ${response.status}`
    try {
      const errorData = await response.json()
      errorMessage =
        errorData.error_description ||
        errorData.message ||
        errorData.title ||
        errorMessage
    } catch {
      // JSON 解析失败，尝试读取纯文本
      try {
        const text = await response.text()
        if (text) errorMessage = text
      } catch {
        // 忽略
      }
    }

    // 401 且有 token 且非登录请求：会话过期，清除状态并跳转登录页
    if (response.status === 401 && !skipAuthRedirect) {
      const token = getToken()
      if (token) {
        const store = await getUserStore()
        const menuStore = await getMenuStore()
        store.forceLogout()
        menuStore.resetMenu()
        const router = await getRouter()
        if (router.currentRoute.value.name !== 'Login') {
          router.push({ name: 'Login' })
        }
      }
    }

    throw new Error(errorMessage)
  }

  const contentType = response.headers.get('content-type')
  if (contentType && contentType.includes('application/json')) {
    const json = await response.json()
    return rawResponse ? json : transformKeysToCamel<T>(json)
  }

  return null as unknown as T
}

/** 请求默认超时时间（30秒） */
const DEFAULT_TIMEOUT = 30_000

/**
 * 统一请求方法
 * Real API 实现类调用此方法发起 HTTP 请求
 */
export async function request<TResponse>(config: RequestConfig): Promise<TResponse> {
  const {
    url,
    method = 'POST',
    data,
    rawResponse = false,
    skipAuthRedirect = false,
    forceBasicAuth = false
  } = config

  const token = getToken()

  let requestUrl = url
  let authHeader: string

  // forceBasicAuth 或无 token 时使用 HMAC Basic 认证
  if (token && !forceBasicAuth) {
    authHeader = `Bearer ${token}`
  } else {
    // 使用 HMAC-SHA512 签名的 Basic 认证
    const timestamp = getTimestamp()
    const separator = url.includes('?') ? '&' : '?'
    requestUrl = `${url}${separator}timestamp=${timestamp}`
    authHeader = generateBasicAuth(url, undefined, timestamp)
  }

  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    'Authorization': authHeader
  }

  const fetchConfig: RequestInit = {
    method,
    headers
  }

  // 处理请求体
  if (data !== undefined) {
    if (typeof data === 'string') {
      // 表单格式（如 URLSearchParams.toString()）
      fetchConfig.body = data
      headers['Content-Type'] = 'application/x-www-form-urlencoded'
    } else {
      // JSON 格式，自动将 camelCase key 转为 snake_case
      fetchConfig.body = JSON.stringify(transformKeysToSnake(data))
    }
  }

  const controller = new AbortController()
  const timeoutId = setTimeout(() => controller.abort(), DEFAULT_TIMEOUT)

  try {
    const response = await fetch(`${API_BASE_URL}${requestUrl}`, {
      ...fetchConfig,
      signal: controller.signal
    })
    clearTimeout(timeoutId)
    return processResponse<TResponse>(response, rawResponse, skipAuthRedirect)
  } catch (error) {
    clearTimeout(timeoutId)
    if (error instanceof DOMException && error.name === 'AbortError') {
      throw new Error('请求超时，请稍后重试')
    }
    if (error instanceof TypeError && error.message.includes('fetch')) {
      throw new Error('网络连接失败，请检查网络')
    }
    throw error
  }
}
