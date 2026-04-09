/**
 * 权限管理 API
 */
import { post } from './request'

/**
 * 获取权限列表
 * @param {object} [params] - 查询参数
 * @param {string} [params.category] - 分类筛选
 */
export function getPermissionList(params = {}) {
  return post('/api/manager/permission/list', params)
}

/**
 * 获取权限分类列表
 */
export function getPermissionCategories() {
  return post('/api/manager/permission/categories')
}

export default {
  getPermissionList,
  getPermissionCategories
}
