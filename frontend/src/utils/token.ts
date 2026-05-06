/**
 * Token 存取工具
 * Web 端使用 localStorage
 */

const TOKEN_KEY = 'token'
const REFRESH_TOKEN_KEY = 'refreshToken'

/** 获取访问令牌 */
export function getToken(): string {
  return localStorage.getItem(TOKEN_KEY) || ''
}

/** 保存访问令牌 */
export function setToken(token: string): void {
  localStorage.setItem(TOKEN_KEY, token)
}

/** 获取刷新令牌 */
export function getRefreshToken(): string {
  return localStorage.getItem(REFRESH_TOKEN_KEY) || ''
}

/** 保存刷新令牌 */
export function setRefreshToken(token: string): void {
  localStorage.setItem(REFRESH_TOKEN_KEY, token)
}

/** 清除所有令牌 */
export function clearToken(): void {
  localStorage.removeItem(TOKEN_KEY)
  localStorage.removeItem(REFRESH_TOKEN_KEY)
}

/** 是否存在访问令牌 */
export function hasToken(): boolean {
  return !!localStorage.getItem(TOKEN_KEY)
}
