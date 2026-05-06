/**
 * 系统菜单管理 API 模块
 *
 * 提供管理端系统菜单的 CRUD、树形结构、角色菜单分配等接口
 * 策略工厂模式：Real 调用后端 API，Mock 使用本地模拟数据
 */

import { request } from '@/api/request'
import { MOCK_ENABLED } from '@/config'
import { mockSystemMenuList, mockSystemMenuTree } from '@/mock/data/manager/system-menu'

// ---- 类型定义 ----

/** 系统菜单查询参数 */
export interface SystemMenuQueryParams {
  /** 菜单名称（模糊搜索） */
  name?: string
}

/** 系统菜单项（含子菜单，树形结构） */
export interface SystemMenuItem {
  /** 菜单 ID */
  id: number
  /** 菜单名称 */
  name: string
  /** 路由路径 */
  path: string
  /** 图标 */
  icon: string
  /** 父级菜单 ID（顶级为 null） */
  parentId: number | null
  /** 排序序号 */
  order: number
  /** 所需权限标识 */
  permission: string
  /** 子菜单列表 */
  children: SystemMenuItem[]
}

/** 创建系统菜单参数 */
export interface CreateSystemMenuParams {
  /** 菜单名称 */
  name: string
  /** 路由路径 */
  path: string
  /** 图标（可选） */
  icon?: string
  /** 父级菜单 ID（可选，顶级菜单不传） */
  parentId?: number
  /** 所需权限标识（可选） */
  permission?: string
}

/** 更新系统菜单参数 */
export interface UpdateSystemMenuParams {
  /** 菜单 ID */
  id: number
  /** 菜单名称（可选） */
  name?: string
  /** 路由路径（可选） */
  path?: string
  /** 图标（可选） */
  icon?: string
  /** 所需权限标识（可选） */
  permission?: string
}

// ---- 接口契约（Strategy Interface）----

/** 系统菜单管理 API 接口契约 */
export interface ISystemMenuApi {
  /**
   * 获取系统菜单列表（扁平）
   * @param params - 查询参数
   * @returns 系统菜单列表
   */
  getSystemMenuList(params?: SystemMenuQueryParams): Promise<SystemMenuItem[]>
  /**
   * 获取系统菜单树形结构
   * @returns 树形菜单列表
   */
  getSystemMenuTree(): Promise<SystemMenuItem[]>
  /**
   * 创建系统菜单
   * @param data - 创建参数
   * @returns 新创建的菜单
   */
  createSystemMenu(data: CreateSystemMenuParams): Promise<SystemMenuItem>
  /**
   * 更新系统菜单
   * @param data - 更新参数
   * @returns 更新后的菜单
   */
  updateSystemMenu(data: UpdateSystemMenuParams): Promise<SystemMenuItem>
  /**
   * 删除系统菜单
   * @param id - 菜单 ID
   */
  deleteSystemMenu(id: number): Promise<void>
  /**
   * 获取角色已分配的系统菜单列表
   * @param roleId - 角色 ID
   * @returns 菜单 ID 列表
   */
  getRoleMenus(roleId: number): Promise<number[]>
  /**
   * 为角色分配系统菜单
   * @param roleId - 角色 ID
   * @param menuIds - 菜单 ID 列表
   */
  assignRoleMenus(roleId: number, menuIds: number[]): Promise<void>
}

// ---- Real 实现（适配 HTTP）----

class RealSystemMenuApi implements ISystemMenuApi {
  async getSystemMenuList(params?: SystemMenuQueryParams) {
    return request<SystemMenuItem[]>({
      url: '/api/manager/system-menu/list',
      method: 'POST',
      data: params ?? {}
    })
  }

  async getSystemMenuTree() {
    return request<SystemMenuItem[]>({
      url: '/api/manager/system-menu/tree',
      method: 'POST'
    })
  }

  async createSystemMenu(data: CreateSystemMenuParams) {
    return request<SystemMenuItem>({
      url: '/api/manager/system-menu/create',
      method: 'POST',
      data
    })
  }

  async updateSystemMenu(data: UpdateSystemMenuParams) {
    return request<SystemMenuItem>({
      url: '/api/manager/system-menu/update',
      method: 'POST',
      data
    })
  }

  async deleteSystemMenu(id: number) {
    await request<void>({
      url: '/api/manager/system-menu/delete',
      method: 'POST',
      data: { id }
    })
  }

  async getRoleMenus(roleId: number) {
    return request<number[]>({
      url: `/api/manager/system-menu/role/${roleId}/menus`,
      method: 'POST'
    })
  }

  async assignRoleMenus(roleId: number, menuIds: number[]) {
    await request<void>({
      url: `/api/manager/system-menu/role/${roleId}/assign-menus`,
      method: 'POST',
      data: { menuIds }
    })
  }
}

// ---- Mock 实现（适配本地数据）----

/** 辅助：扁平化树形菜单 */
function flattenMenuTree(tree: SystemMenuItem[]): SystemMenuItem[] {
  const result: SystemMenuItem[] = []
  for (const item of tree) {
    result.push(item)
    if (item.children?.length) {
      result.push(...flattenMenuTree(item.children))
    }
  }
  return result
}

class MockSystemMenuApi implements ISystemMenuApi {
  async getSystemMenuList(params?: SystemMenuQueryParams): Promise<SystemMenuItem[]> {
    const flat = flattenMenuTree(mockSystemMenuTree)
    if (!params?.name) return flat
    const kw = params.name.toLowerCase()
    return flat.filter(m => m.name.toLowerCase().includes(kw))
  }

  async getSystemMenuTree(): Promise<SystemMenuItem[]> {
    return [...mockSystemMenuTree]
  }

  async createSystemMenu(data: CreateSystemMenuParams): Promise<SystemMenuItem> {
    return {
      id: Date.now(),
      name: data.name,
      path: data.path,
      icon: data.icon ?? '',
      parentId: data.parentId ?? null,
      order: 0,
      permission: data.permission ?? '',
      children: []
    }
  }

  async updateSystemMenu(data: UpdateSystemMenuParams): Promise<SystemMenuItem> {
    const flat = flattenMenuTree(mockSystemMenuTree)
    const existing = flat.find(m => m.id === data.id)
    if (!existing) throw new Error(`系统菜单 ${data.id} 不存在`)
    return {
      ...existing,
      name: data.name ?? existing.name,
      path: data.path ?? existing.path,
      icon: data.icon ?? existing.icon,
      permission: data.permission ?? existing.permission
    }
  }

  async deleteSystemMenu(id: number): Promise<void> {
    const flat = flattenMenuTree(mockSystemMenuTree)
    const existing = flat.find(m => m.id === id)
    if (!existing) throw new Error(`系统菜单 ${id} 不存在`)
  }

  async getRoleMenus(_roleId: number): Promise<number[]> {
    // Mock：返回所有顶级菜单 ID
    return mockSystemMenuTree.map(m => m.id)
  }

  async assignRoleMenus(_roleId: number, _menuIds: number[]): Promise<void> {
    // Mock 模式下不实际修改数据
  }
}

// ---- 工厂函数（Simple Factory）----

/** 创建系统菜单管理 API 实例 */
export function createSystemMenuApi(): ISystemMenuApi {
  return MOCK_ENABLED ? new MockSystemMenuApi() : new RealSystemMenuApi()
}

// ---- 模块实例（模块级单例）----

/** 系统菜单管理 API 单例 */
export const systemMenuApi = createSystemMenuApi()
