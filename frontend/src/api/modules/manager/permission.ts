/**
 * 权限查询 API 模块
 *
 * 提供管理端权限列表、分类查询接口
 * 策略工厂模式：Real 调用后端 API，Mock 使用本地模拟数据
 */

import { request } from '@/api/request'
import { MOCK_ENABLED } from '@/config'
import { mockPermissionList, mockPermissionCategories } from '@/mock/data/manager/permission'

// ---- 类型定义 ----

/** 权限查询参数 */
export interface PermissionQueryParams {
  /** 权限分类 */
  category?: string
}

/** 权限项 */
export interface PermissionItem {
  /** 权限 ID */
  id: number
  /** 权限编码 */
  code: string
  /** 权限名称 */
  name: string
  /** 权限分类 */
  category: string
  /** 权限描述 */
  description: string
}

/** 权限分类统计 */
export interface PermissionCategory {
  /** 分类名称 */
  name: string
  /** 该分类下的权限数量 */
  count: number
}

// ---- 接口契约（Strategy Interface）----

/** 权限查询 API 接口契约 */
export interface IPermissionApi {
  /**
   * 获取权限列表
   * @param params - 查询参数（可选，按分类过滤）
   * @returns 权限列表
   */
  getPermissionList(params?: PermissionQueryParams): Promise<PermissionItem[]>
  /**
   * 获取权限分类统计列表
   * @returns 分类统计列表
   */
  getPermissionCategories(): Promise<PermissionCategory[]>
}

// ---- Real 实现（适配 HTTP）----

class RealPermissionApi implements IPermissionApi {
  async getPermissionList(params?: PermissionQueryParams) {
    return request<PermissionItem[]>({
      url: '/api/manager/permission/list',
      method: 'POST',
      data: params ?? {}
    })
  }

  async getPermissionCategories() {
    return request<PermissionCategory[]>({
      url: '/api/manager/permission/categories',
      method: 'POST'
    })
  }
}

// ---- Mock 实现（适配本地数据）----

class MockPermissionApi implements IPermissionApi {
  async getPermissionList(params?: PermissionQueryParams): Promise<PermissionItem[]> {
    if (!params?.category) return [...mockPermissionList]
    return mockPermissionList.filter(p => p.category === params.category)
  }

  async getPermissionCategories(): Promise<PermissionCategory[]> {
    return [...mockPermissionCategories]
  }
}

// ---- 工厂函数（Simple Factory）----

/** 创建权限查询 API 实例 */
export function createPermissionApi(): IPermissionApi {
  return MOCK_ENABLED ? new MockPermissionApi() : new RealPermissionApi()
}

// ---- 模块实例（模块级单例）----

/** 权限查询 API 单例 */
export const permissionApi = createPermissionApi()
