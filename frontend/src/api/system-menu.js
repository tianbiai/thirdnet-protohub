/**
 * 系统功能菜单 API
 */
import { post } from './request'

/**
 * 获取系统功能菜单列表
 * @param {object} params - 查询参数
 */
export function getSystemMenuList(params = {}) {
  return post('/api/manager/system-menu/list', params)
}

/**
 * 获取系统功能菜单树形结构
 */
export function getSystemMenuTree() {
  return post('/api/manager/system-menu/tree')
}

/**
 * 创建系统功能菜单
 * @param {object} data - 菜单数据
 */
export function createSystemMenu(data) {
  return post('/api/manager/system-menu/create', data)
}

/**
 * 更新系统功能菜单
 * @param {object} data - 菜单数据
 */
export function updateSystemMenu(data) {
  return post('/api/manager/system-menu/update', data)
}

/**
 * 删除系统功能菜单
 * @param {number} id - 菜单 ID
 */
export function deleteSystemMenu(id) {
  return post('/api/manager/system-menu/delete', { id })
}

/**
 * 获取角色可见的功能菜单
 * @param {number} roleId - 角色 ID
 */
export function getRoleMenus(roleId) {
  return post(`/api/manager/system-menu/role/${roleId}/menus`)
}

/**
 * 分配角色功能菜单
 * @param {number} roleId - 角色 ID
 * @param {Array<number>} menuIds - 菜单 ID 列表
 */
export function assignRoleMenus(roleId, menuIds) {
  return post(`/api/manager/system-menu/role/${roleId}/assign-menus`, { menuIds })
}

/**
 * 获取当前用户可见的功能菜单
 */
export function getMyMenus() {
  return post('/api/app/system-menu/my-menus')
}

export default {
  getSystemMenuList,
  getSystemMenuTree,
  createSystemMenu,
  updateSystemMenu,
  deleteSystemMenu,
  getRoleMenus,
  assignRoleMenus,
  getMyMenus
}
