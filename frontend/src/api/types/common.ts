/**
 * API 通用类型定义
 */

/** 端点标识：app（用户端）、manager（管理端） */
export type Endpoint = 'manager' | 'app'

/** 分页请求参数 */
export interface PaginationParams {
  /** 页码（从 1 开始） */
  page?: number
  /** 每页数量 */
  pageSize?: number
}

/** 分页响应结构 */
export interface PaginatedResponse<T> {
  /** 数据列表 */
  list: T[]
  /** 总记录数 */
  total: number
  /** 当前页码 */
  page: number
  /** 每页数量 */
  pageSize: number
}

/** 请求配置 */
export interface RequestConfig<TData = unknown> {
  /** 请求路径（相对路径） */
  url: string
  /** HTTP 方法（网关限制仅支持 GET/POST） */
  method: 'GET' | 'POST'
  /** 请求体数据 */
  data?: TData
  /** URL 查询参数 */
  params?: Record<string, unknown>
  /** 是否跳过响应 key 转换（如 token 接口） */
  rawResponse?: boolean
  /** 是否跳过 401 自动跳转（如登录、刷新令牌请求） */
  skipAuthRedirect?: boolean
  /** 是否强制使用 HMAC Basic 认证（如登录、刷新令牌） */
  forceBasicAuth?: boolean
}

/** API 错误 */
export interface ApiError {
  /** HTTP 状态码 */
  status: number
  /** 错误消息 */
  message: string
}
