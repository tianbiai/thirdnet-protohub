/**
 * 菜单相关 API
 */
import { post } from './request'

// ==================== 应用端接口 ====================

/**
 * 获取菜单配置
 * @returns {Promise<{title: string, version: string, groups: Array}>}
 */
export function getMenuConfig() {
  return post('/api/app/menu/list')
}

// ==================== 管理端 - 分组接口 ====================

/**
 * 获取分组列表
 * @returns {Promise<Array>}
 */
export function getGroupList() {
  return post('/api/manager/menu/group/list')
}

/**
 * 创建分组
 * @param {object} data - 分组数据
 * @param {string} data.name - 分组名称
 * @param {string} [data.icon] - 图标
 */
export function createGroup(data) {
  return post('/api/manager/menu/group/create', data)
}

/**
 * 更新分组
 * @param {object} data - 分组数据
 * @param {number} data.id - 分组 ID
 * @param {string} [data.name] - 分组名称
 * @param {string} [data.icon] - 图标
 */
export function updateGroup(data) {
  return post('/api/manager/menu/group/update', data)
}

/**
 * 删除分组
 * @param {number} id - 分组 ID
 */
export function deleteGroup(id) {
  return post('/api/manager/menu/group/delete', { id })
}

/**
 * 分组排序
 * @param {Array<{id: number, order: number}>} orders - 排序数据
 */
export function reorderGroups(orders) {
  return post('/api/manager/menu/group/reorder', { ids: orders.map(o => o.id) })
}

// ==================== 管理端 - 菜单项接口 ====================

/**
 * 获取分组下的菜单项列表
 * @param {number} groupid - 分组 ID
 */
export function getItemList(groupid) {
  return post('/api/manager/menu/item/list', { groupid })
}

/**
 * 创建菜单项
 * @param {object} data - 菜单项数据
 * @param {number} data.groupid - 所属分组 ID
 * @param {string} data.name - 名称
 * @param {string} data.type - 类型
 * @param {string} [data.url] - 链接地址
 * @param {string} [data.icon] - 图标
 * @param {string} [data.description] - 描述
 * @param {object} [data.viewport] - 视口配置
 */
export function createItem(data) {
  return post('/api/manager/menu/item/create', data)
}

/**
 * 更新菜单项
 * @param {object} data - 菜单项数据
 */
export function updateItem(data) {
  return post('/api/manager/menu/item/update', data)
}

/**
 * 删除菜单项
 * @param {number} id - 菜单项 ID
 */
export function deleteItem(id) {
  return post('/api/manager/menu/item/delete', { id })
}

/**
 * 菜单项排序
 * @param {number} groupid - 分组 ID
 * @param {Array<{id: number, order: number}>} orders - 排序数据
 */
export function reorderItems(groupid, orders) {
  return post('/api/manager/menu/item/reorder', { ids: orders.map(o => o.id) })
}

export default {
  // 应用端
  getMenuConfig,
  // 分组管理
  getGroupList,
  createGroup,
  updateGroup,
  deleteGroup,
  reorderGroups,
  // 菜单项管理
  getItemList,
  createItem,
  updateItem,
  deleteItem,
  reorderItems
}
