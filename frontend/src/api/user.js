/**
 * 用户管理 API
 */
import { post } from './request'

/**
 * 获取用户列表（分页）
 * @param {object} params - 查询参数
 * @param {number} [params.page] - 页码
 * @param {number} [params.pageSize] - 每页数量
 * @param {string} [params.keyword] - 搜索关键词
 * @param {number} [params.status] - 状态筛选
 */
export function getUserList(params = {}) {
  return post('/api/manager/user/list', params)
}

/**
 * 创建用户
 * @param {object} data - 用户数据
 * @param {string} data.username - 用户名
 * @param {string} data.password - 密码
 * @param {string} [data.nickname] - 昵称
 * @param {string} [data.email] - 邮箱
 * @param {number} [data.status] - 状态（0=禁用, 1=启用）
 * @param {string} [data.description] - 描述
 */
export function createUser(data) {
  return post('/api/manager/user/create', data)
}

/**
 * 更新用户
 * @param {object} data - 用户数据
 * @param {number} data.id - 用户 ID
 */
export function updateUser(data) {
  return post('/api/manager/user/update', data)
}

/**
 * 删除用户
 * @param {number} id - 用户 ID
 */
export function deleteUser(id) {
  return post('/api/manager/user/delete', { id })
}

/**
 * 获取用户的角色列表
 * @param {number} userId - 用户 ID
 */
export function getUserRoles(userId) {
  return post(`/api/manager/user/${userId}/roles`)
}

/**
 * 为用户分配角色
 * @param {number} userId - 用户 ID
 * @param {Array<number>} roleIds - 角色 ID 列表
 */
export function assignUserRoles(userId, roleIds) {
  return post(`/api/manager/user/${userId}/assign-roles`, { roleIds })
}

export default {
  getUserList,
  createUser,
  updateUser,
  deleteUser,
  getUserRoles,
  assignUserRoles
}
