/**
 * 角色管理 API
 */
import { post } from './request'

/**
 * 获取角色列表
 * @param {object} [params] - 查询参数
 */
export function getRoleList(params = {}) {
  return post('/api/manager/role/list', params)
}

/**
 * 获取角色详情
 * @param {number} id - 角色 ID
 */
export function getRoleDetail(id) {
  return post(`/api/manager/role/${id}/detail`)
}

/**
 * 创建角色
 * @param {object} data - 角色数据
 * @param {string} data.code - 角色编码
 * @param {string} data.name - 角色名称
 * @param {string} [data.description] - 描述
 */
export function createRole(data) {
  return post('/api/manager/role/create', data)
}

/**
 * 更新角色
 * @param {object} data - 角色数据
 * @param {number} data.id - 角色 ID
 */
export function updateRole(data) {
  return post('/api/manager/role/update', data)
}

/**
 * 删除角色
 * @param {number} id - 角色 ID
 */
export function deleteRole(id) {
  return post('/api/manager/role/delete', { id })
}

/**
 * 获取角色的权限列表
 * @param {number} roleId - 角色 ID
 */
export function getRolePermissions(roleId) {
  return post(`/api/manager/role/${roleId}/permissions`)
}

/**
 * 为角色分配权限
 * @param {number} roleId - 角色 ID
 * @param {Array<number>} permissionIds - 权限 ID 列表
 */
export function assignRolePermissions(roleId, permissionIds) {
  return post(`/api/manager/role/${roleId}/assign-permissions`, { permissionIds })
}

export default {
  getRoleList,
  getRoleDetail,
  createRole,
  updateRole,
  deleteRole,
  getRolePermissions,
  assignRolePermissions
}
