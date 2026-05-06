/**
 * 菜单管理 API 模块
 *
 * 提供管理端菜单分组和菜单项的 CRUD、排序等接口
 * 策略工厂模式：Real 调用后端 API，Mock 使用本地模拟数据
 */

import { request } from '@/api/request'
import { MOCK_ENABLED } from '@/config'
import { mockMenuGroupList, mockMenuItemList } from '@/mock/data/manager/menu'

// ---- 类型定义 ----

/** 菜单分组项 */
export interface MenuGroupItem {
  /** 分组 ID */
  id: number
  /** 分组名称 */
  name: string
  /** 分组图标 */
  icon: string
  /** 排序序号 */
  order: number
  /** 分组下菜单项数量 */
  itemCount: number
}

/** 菜单项详情 */
export interface MenuItemDetail {
  /** 菜单项 ID */
  id: number
  /** 所属分组 ID */
  groupId: number
  /** 菜单名称 */
  name: string
  /** 菜单类型（web/miniprogram/link/doc/internal） */
  type: string
  /** 菜单 URL */
  url: string
  /** 菜单描述 */
  description: string
  /** 排序序号 */
  order: number
  /** 视口配置（小程序类型使用） */
  viewport: { width: number; height: number } | null
  /** 内部路由路径 */
  route: string
}

/** 创建菜单分组参数 */
export interface CreateGroupParams {
  /** 分组名称 */
  name: string
  /** 分组图标（可选） */
  icon?: string
}

/** 更新菜单分组参数 */
export interface UpdateGroupParams {
  /** 分组 ID */
  id: number
  /** 分组名称（可选） */
  name?: string
  /** 分组图标（可选） */
  icon?: string
}

/** 创建菜单项参数 */
export interface CreateItemParams {
  /** 所属分组 ID */
  groupId: number
  /** 菜单名称 */
  name: string
  /** 菜单类型 */
  type: string
  /** 菜单 URL（可选） */
  url?: string
  /** 菜单描述（可选） */
  description?: string
  /** 视口配置（可选） */
  viewport?: { width: number; height: number }
  /** 内部路由路径（可选） */
  route?: string
}

/** 更新菜单项参数 */
export interface UpdateItemParams {
  /** 菜单项 ID */
  id: number
  /** 菜单名称（可选） */
  name?: string
  /** 菜单类型（可选） */
  type?: string
  /** 菜单 URL（可选） */
  url?: string
  /** 菜单描述（可选） */
  description?: string
  /** 视口配置（可选） */
  viewport?: { width: number; height: number }
  /** 内部路由路径（可选） */
  route?: string
}

// ---- 接口契约（Strategy Interface）----

/** 菜单管理 API 接口契约 */
export interface IMenuApi {
  // ---- 分组操作 ----

  /**
   * 获取菜单分组列表
   * @returns 分组列表
   */
  getGroupList(): Promise<MenuGroupItem[]>
  /**
   * 创建菜单分组
   * @param data - 创建参数
   * @returns 新创建的分组
   */
  createGroup(data: CreateGroupParams): Promise<MenuGroupItem>
  /**
   * 更新菜单分组
   * @param data - 更新参数
   * @returns 更新后的分组
   */
  updateGroup(data: UpdateGroupParams): Promise<MenuGroupItem>
  /**
   * 删除菜单分组
   * @param id - 分组 ID
   */
  deleteGroup(id: number): Promise<void>
  /**
   * 分组排序
   * @param ids - 按顺序排列的分组 ID 列表
   */
  reorderGroups(ids: number[]): Promise<void>

  // ---- 菜单项操作 ----

  /**
   * 获取指定分组下的菜单项列表
   * @param groupId - 分组 ID
   * @returns 菜单项列表
   */
  getItemList(groupId: number): Promise<MenuItemDetail[]>
  /**
   * 创建菜单项
   * @param data - 创建参数
   * @returns 新创建的菜单项
   */
  createItem(data: CreateItemParams): Promise<MenuItemDetail>
  /**
   * 更新菜单项
   * @param data - 更新参数
   * @returns 更新后的菜单项
   */
  updateItem(data: UpdateItemParams): Promise<MenuItemDetail>
  /**
   * 删除菜单项
   * @param id - 菜单项 ID
   */
  deleteItem(id: number): Promise<void>
  /**
   * 菜单项排序
   * @param ids - 按顺序排列的菜单项 ID 列表
   */
  reorderItems(ids: number[]): Promise<void>
}

// ---- Real 实现（适配 HTTP）----

class RealMenuApi implements IMenuApi {
  // ---- 分组操作 ----

  async getGroupList() {
    return request<MenuGroupItem[]>({
      url: '/api/manager/menu/group/list',
      method: 'POST'
    })
  }

  async createGroup(data: CreateGroupParams) {
    return request<MenuGroupItem>({
      url: '/api/manager/menu/group/create',
      method: 'POST',
      data
    })
  }

  async updateGroup(data: UpdateGroupParams) {
    return request<MenuGroupItem>({
      url: '/api/manager/menu/group/update',
      method: 'POST',
      data
    })
  }

  async deleteGroup(id: number) {
    await request<void>({
      url: '/api/manager/menu/group/delete',
      method: 'POST',
      data: { id }
    })
  }

  async reorderGroups(ids: number[]) {
    await request<void>({
      url: '/api/manager/menu/group/reorder',
      method: 'POST',
      data: { ids }
    })
  }

  // ---- 菜单项操作 ----

  async getItemList(groupId: number) {
    return request<MenuItemDetail[]>({
      url: '/api/manager/menu/item/list',
      method: 'POST',
      data: { groupId }
    })
  }

  async createItem(data: CreateItemParams) {
    return request<MenuItemDetail>({
      url: '/api/manager/menu/item/create',
      method: 'POST',
      data
    })
  }

  async updateItem(data: UpdateItemParams) {
    return request<MenuItemDetail>({
      url: '/api/manager/menu/item/update',
      method: 'POST',
      data
    })
  }

  async deleteItem(id: number) {
    await request<void>({
      url: '/api/manager/menu/item/delete',
      method: 'POST',
      data: { id }
    })
  }

  async reorderItems(ids: number[]) {
    await request<void>({
      url: '/api/manager/menu/item/reorder',
      method: 'POST',
      data: { ids }
    })
  }
}

// ---- Mock 实现（适配本地数据）----

class MockMenuApi implements IMenuApi {
  // ---- 分组操作 ----

  async getGroupList(): Promise<MenuGroupItem[]> {
    return [...mockMenuGroupList]
  }

  async createGroup(data: CreateGroupParams): Promise<MenuGroupItem> {
    return {
      id: Date.now(),
      name: data.name,
      icon: data.icon ?? '',
      order: mockMenuGroupList.length + 1,
      itemCount: 0
    }
  }

  async updateGroup(data: UpdateGroupParams): Promise<MenuGroupItem> {
    const existing = mockMenuGroupList.find(g => g.id === data.id)
    if (!existing) throw new Error(`分组 ${data.id} 不存在`)
    return {
      ...existing,
      name: data.name ?? existing.name,
      icon: data.icon ?? existing.icon
    }
  }

  async deleteGroup(id: number): Promise<void> {
    const existing = mockMenuGroupList.find(g => g.id === id)
    if (!existing) throw new Error(`分组 ${id} 不存在`)
  }

  async reorderGroups(_ids: number[]): Promise<void> {
    // Mock 模式下不实际修改数据
  }

  // ---- 菜单项操作 ----

  async getItemList(groupId: number): Promise<MenuItemDetail[]> {
    return mockMenuItemList.filter(item => item.groupId === groupId)
  }

  async createItem(data: CreateItemParams): Promise<MenuItemDetail> {
    const groupItems = mockMenuItemList.filter(item => item.groupId === data.groupId)
    return {
      id: Date.now(),
      groupId: data.groupId,
      name: data.name,
      type: data.type,
      url: data.url ?? '',
      description: data.description ?? '',
      order: groupItems.length + 1,
      viewport: data.viewport ?? null,
      route: data.route ?? ''
    }
  }

  async updateItem(data: UpdateItemParams): Promise<MenuItemDetail> {
    const existing = mockMenuItemList.find(item => item.id === data.id)
    if (!existing) throw new Error(`菜单项 ${data.id} 不存在`)
    return {
      ...existing,
      name: data.name ?? existing.name,
      type: data.type ?? existing.type,
      url: data.url ?? existing.url,
      description: data.description ?? existing.description,
      viewport: data.viewport ?? existing.viewport,
      route: data.route ?? existing.route
    }
  }

  async deleteItem(id: number): Promise<void> {
    const existing = mockMenuItemList.find(item => item.id === id)
    if (!existing) throw new Error(`菜单项 ${id} 不存在`)
  }

  async reorderItems(_ids: number[]): Promise<void> {
    // Mock 模式下不实际修改数据
  }
}

// ---- 工厂函数（Simple Factory）----

/** 创建菜单管理 API 实例 */
export function createManagerMenuApi(): IMenuApi {
  return MOCK_ENABLED ? new MockMenuApi() : new RealMenuApi()
}

// ---- 模块实例（模块级单例）----

/** 菜单管理 API 单例 */
export const managerMenuApi = createManagerMenuApi()
