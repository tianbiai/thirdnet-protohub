/**
 * 认证相关 API
 */
import { post, postForm } from './request'

/**
 * 获取 JWT Token（通过账号密码验证）
 * @param {string} username - 用户名
 * @param {string} password - 密码
 * @param {string} scope - 权限范围（默认 'protohub'）
 * @returns {Promise<{access_token: string, refresh_token?: string}>}
 */
export async function getToken(username, password, scope = 'protohub offline_access') {
  const formData = new URLSearchParams()
  formData.append('username', username)
  formData.append('password', password)
  formData.append('scope', scope)
  return postForm('/connect/token', formData)
}

/**
 * 用户登出
 */
export function logout() {
  return post('/api/app/auth/logout')
}

/**
 * 使用 refresh_token 刷新 access_token
 * @param {string} rt - refresh-token
 * @returns {Promise<{access_token: string, refresh_token: string}>}
 */
export async function refreshToken(rt) {
  const formData = new URLSearchParams()
  formData.append('refresh_token', rt)
  return postForm('/connect/token/refresh', formData)
}

/**
 * 获取当前用户信息
 * @returns {Promise<object>}
 */
export function getCurrentUser() {
  return post('/api/app/auth/me')
}

/**
 * 修改密码
 * @param {string} oldpassword - 旧密码
 * @param {string} newpassword - 新密码
 */
export function changePassword(oldpassword, newpassword) {
  return post('/api/app/auth/change-password', { oldpassword, newpassword })
}

export default {
  getToken,
  refreshToken,
  logout,
  getCurrentUser,
  changePassword
}
