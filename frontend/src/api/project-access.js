/**
 * 项目访问管理 API
 */
import { post } from './request'

/**
 * 获取项目授权列表
 * @param {object} [params] - 查询参数
 * @param {number} [params.projectId] - 项目 ID 筛选
 * @param {number} [params.userId] - 用户 ID 筛选
 */
export function getAccessList(params = {}) {
  return post('/api/manager/project-access/list', params)
}

/**
 * 授予项目访问权限
 * @param {object} data - 授权数据
 * @param {number} data.userId - 用户 ID
 * @param {number} data.projectId - 项目 ID
 * @param {string} [data.accessType] - 访问类型（view/manage）
 */
export function grantAccess(data) {
  return post('/api/manager/project-access/grant', data)
}

/**
 * 撤销项目访问权限
 * @param {number} id - 访问记录 ID
 */
export function revokeAccess(id) {
  return post('/api/manager/project-access/revoke', { id })
}

/**
 * 获取用户的项目访问列表
 * @param {number} userId - 用户 ID
 */
export function getUserProjects(userId) {
  return post(`/api/manager/project-access/user/${userId}/projects`)
}

/**
 * 获取当前用户可管理的项目列表
 */
export function getMyProjects() {
  return post('/api/manager/project-access/my-projects')
}

export default {
  getAccessList,
  grantAccess,
  revokeAccess,
  getUserProjects,
  getMyProjects
}
