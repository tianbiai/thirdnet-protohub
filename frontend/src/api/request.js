/**
 * API 请求封装模块
 *
 * 认证策略：
 * - 已登录（有 JWT token）：使用 Bearer token 认证
 * - 未登录（如获取 token 前的请求）：使用 HMAC-SHA512 签名的 Basic 认证
 *
 * 401 时自动跳转登录页
 */

import { transformKeysToCamel, transformKeysToSnake } from '@/utils/transform'
import { generateBasicAuth, getTimestamp } from '@/utils/signature'
import router from '@/router'
import { useUserStore } from '@/stores/user'
import { useMenuStore } from '@/stores/menu'

// API 基础路径
const BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:6003'

/**
 * 处理 fetch 响应（非 401）
 * @param {Response} response
 * @param {boolean} rawResponse - 为 true 时不转换 key（用于 token 等保持 snake_case 的接口）
 */
async function processResponse(response, rawResponse = false) {
  if (!response.ok) {
    let errorMessage = `请求失败: ${response.status}`
    try {
      const errorData = await response.json()
      errorMessage = errorData.message || errorData.error_description || errorData.title || errorMessage
    } catch {
      // 无法解析错误信息，使用默认消息
    }
    throw new Error(errorMessage)
  }

  const contentType = response.headers.get('content-type')
  if (contentType && contentType.includes('application/json')) {
    const json = await response.json()
    return rawResponse ? json : transformKeysToCamel(json)
  }

  return null
}

/**
 * 认证失败处理：清除状态并跳转登录页
 */
async function handleAuthExpired() {
  const store = useUserStore()
  const menuStore = useMenuStore()
  store.forceLogout()
  menuStore.resetMenu()
  if (router.currentRoute.value.name !== 'Login') {
    await router.push({ name: 'Login' })
  }
}

/**
 * 通用请求方法
 *
 * 认证策略：
 * - 有 JWT token 时：使用 Bearer 认证
 * - 无 JWT token 时：使用 HMAC-SHA512 签名的 Basic 认证（含 timestamp 参数）
 *
 * @param {string} url - 请求路径（相对路径，不含域名）
 * @param {object} options - 请求选项
 * @param {boolean} options.rawResponse - 为 true 时不转换响应（用于 token 等接口）
 * @param {string|object} options.body - 请求体，字符串类型直接发送，对象类型转 JSON
 * @returns {Promise<any>}
 */
async function request(url, options = {}) {
  const token = localStorage.getItem('token')

  let requestUrl = url
  let authHeader

  if (token) {
    // 已登录：使用 Bearer token
    authHeader = `Bearer ${token}`
  } else {
    // 未登录：使用 HMAC-SHA512 签名的 Basic 认证
    const timestamp = getTimestamp()
    const separator = url.includes('?') ? '&' : '?'
    requestUrl = `${url}${separator}timestamp=${timestamp}`
    authHeader = generateBasicAuth(url, undefined, timestamp)
  }

  const isFormBody = typeof options.body === 'string'
  const defaultHeaders = {
    'Content-Type': isFormBody ? 'application/x-www-form-urlencoded' : 'application/json',
    'Authorization': authHeader
  }

  const config = {
    ...options,
    headers: {
      ...defaultHeaders,
      ...options.headers
    }
  }

  if (config.body && typeof config.body === 'object') {
    config.body = JSON.stringify(transformKeysToSnake(config.body))
  }

  try {
    const response = await fetch(`${BASE_URL}${requestUrl}`, config)

    // 401：认证失败，跳转登录页
    if (response.status === 401) {
      await handleAuthExpired()
      throw new Error('登录已过期，请重新登录')
    }

    return processResponse(response, options.rawResponse)
  } catch (error) {
    if (error.name === 'TypeError' && error.message.includes('fetch')) {
      throw new Error('网络连接失败，请检查网络')
    }
    throw error
  }
}

/**
 * POST 请求（JSON 格式）
 * @param {string} url - 请求路径
 * @param {object} data - 请求数据
 * @param {object} options - 额外选项
 */
export function post(url, data = {}, options = {}) {
  return request(url, {
    method: 'POST',
    body: data,
    ...options
  })
}

/**
 * POST 请求（表单格式，用于 token 等接口）
 * @param {string} url - 请求路径
 * @param {URLSearchParams} formData - 表单数据
 * @param {object} options - 额外选项
 */
export function postForm(url, formData, options = {}) {
  return request(url, {
    method: 'POST',
    body: formData.toString(),
    rawResponse: true,
    ...options
  })
}
