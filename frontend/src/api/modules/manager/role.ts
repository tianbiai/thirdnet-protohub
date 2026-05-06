/**
 * 角色管理 API 模块
 *
 * 提供管理端角色 CRUD、权限分配等接口
 * 策略工厂模式：Real 调用后端 API，Mock 使用本地模拟数据
 */

import { request } from '@/api/request'
import { MOCK_ENABLED } from '@/config'
import { mockRoleList, mockRoleDetail } from '@/mock/data/manager/role'

// ---- 类型定义 ----

/** 角色查询参数 */
export interface RoleQueryParams {
  /** 角色名称（模糊搜索） */
  name?: string
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

/** 角色列表项 */
export interface RoleItem {
  /** 角色 ID */
  id: number
  /** 角色编码 */
  code: string
  /** 角色名称 */
  name: string
  /** 角色描述 */
  description: string
  /** 是否系统内置角色（不可删除） */
  isSystem: boolean
  /** 创建时间 */
  createTime: string
  /** 更新时间 */
  updateTime: string
}

/** 角色详情（含权限列表） */
export interface RoleDetail extends RoleItem {
  /** 已分配的权限列表 */
  permissions: PermissionItem[]
}

/** 创建角色参数 */
export interface CreateRoleParams {
  /** 角色编码 */
  code: string
  /** 角色名称 */
  name: string
  /** 角色描述（可选） */
  description?: string
}

/** 更新角色参数 */
export interface UpdateRoleParams {
  /** 角色 ID */
  id: number
  /** 角色名称 */
  name: string
  /** 角色描述（可选） */
  description?: string
}

// ---- 接口契约（Strategy Interface）----

/** 角色管理 API 接口契约 */
export interface IRoleApi {
  /**
   * 获取角色列表
   * @param params - 查询参数
   * @returns 角色列表
   */
  getRoleList(params?: RoleQueryParams): Promise<RoleItem[]>
  /**
   * 获取角色详情（含权限）
   * @param id - 角色 ID
   * @returns 角色详情
   */
  getRoleDetail(id: number): Promise<RoleDetail>
  /**
   * 创建角色
   * @param data - 创建参数
   * @returns 新创建的角色
   */
  createRole(data: CreateRoleParams): Promise<RoleItem>
  /**
   * 更新角色信息
   * @param data - 更新参数
   * @returns 更新后的角色
   */
  updateRole(data: UpdateRoleParams): Promise<RoleItem>
  /**
   * 删除角色
   * @param id - 角色 ID
   */
  deleteRole(id: number): Promise<void>
  /**
   * 获取角色已分配的权限列表
   * @param roleId - 角色 ID
   * @returns 权限列表
   */
  getRolePermissions(roleId: number): Promise<PermissionItem[]>
  /**
   * 为角色分配权限
   * @param roleId - 角色 ID
   * @param permissionIds - 权限 ID 列表
   */
  assignRolePermissions(roleId: number, permissionIds: number[]): Promise<void>
}

// ---- Real 实现（适配 HTTP）----

class RealRoleApi implements IRoleApi {
  async getRoleList(params?: RoleQueryParams) {
    return request<RoleItem[]>({
      url: '/api/manager/role/list',
      method: 'POST',
      data: params ?? {}
    })
  }

  async getRoleDetail(id: number) {
    return request<RoleDetail>({
      url: `/api/manager/role/${id}/detail`,
      method: 'POST'
    })
  }

  async createRole(data: CreateRoleParams) {
    return request<RoleItem>({
      url: '/api/manager/role/create',
      method: 'POST',
      data
    })
  }

  async updateRole(data: UpdateRoleParams) {
    return request<RoleItem>({
      url: '/api/manager/role/update',
      method: 'POST',
      data
    })
  }

  async deleteRole(id: number) {
    await request<void>({
      url: '/api/manager/role/delete',
      method: 'POST',
      data: { id }
    })
  }

  async getRolePermissions(roleId: number) {
    return request<PermissionItem[]>({
      url: `/api/manager/role/${roleId}/permissions`,
      method: 'POST'
    })
  }

  async assignRolePermissions(roleId: number, permissionIds: number[]) {
    await request<void>({
      url: `/api/manager/role/${roleId}/assign-permissions`,
      method: 'POST',
      data: { permissionIds }
    })
  }
}

// ---- Mock 实现（适配本地数据）----

class MockRoleApi implements IRoleApi {
  async getRoleList(params?: RoleQueryParams): Promise<RoleItem[]> {
    if (!params?.name) return [...mockRoleList]
    const kw = params.name.toLowerCase()
    return mockRoleList.filter(r => r.name.toLowerCase().includes(kw))
  }

  async getRoleDetail(id: number): Promise<RoleDetail> {
    const detail = mockRoleDetail.find(d => d.id === id)
    if (!detail) throw new Error(`角色 ${id} 不存在`)
    return detail
  }

  async createRole(data: CreateRoleParams): Promise<RoleItem> {
    return {
      id: Date.now(),
      code: data.code,
      name: data.name,
      description: data.description ?? '',
      isSystem: false,
      createTime: new Date().toISOString(),
      updateTime: new Date().toISOString()
    }
  }

  async updateRole(data: UpdateRoleParams): Promise<RoleItem> {
    const existing = mockRoleList.find(r => r.id === data.id)
    if (!existing) throw new Error(`角色 ${data.id} 不存在`)
    return {
      ...existing,
      name: data.name,
      description: data.description ?? existing.description,
      updateTime: new Date().toISOString()
    }
  }

  async deleteRole(id: number): Promise<void> {
    const existing = mockRoleList.find(r => r.id === id)
    if (!existing) throw new Error(`角色 ${id} 不存在`)
    if (existing.isSystem) throw new Error('系统内置角色不可删除')
  }

  async getRolePermissions(roleId: number): Promise<PermissionItem[]> {
    const detail = mockRoleDetail.find(d => d.id === roleId)
    if (!detail) throw new Error(`角色 ${roleId} 不存在`)
    return detail.permissions
  }

  async assignRolePermissions(_roleId: number, _permissionIds: number[]): Promise<void> {
    // Mock 模式下不实际修改数据
  }
}

// ---- 工厂函数（Simple Factory）----

/** 创建角色管理 API 实例 */
export function createRoleApi(): IRoleApi {
  return MOCK_ENABLED ? new MockRoleApi() : new RealRoleApi()
}

// ---- 模块实例（模块级单例）----

/** 角色管理 API 单例 */
export const roleApi = createRoleApi()
