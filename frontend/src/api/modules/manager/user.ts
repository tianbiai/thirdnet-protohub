/**
 * 用户管理 API 模块
 *
 * 提供管理端用户 CRUD、角色分配等接口
 * 策略工厂模式：Real 调用后端 API，Mock 使用本地模拟数据
 */

import { request } from '@/api/request'
import { MOCK_ENABLED } from '@/config'
import type { PaginatedResponse } from '@/api/types/common'
import { mockUserList } from '@/mock/data/manager/user'

// ---- 枚举类型 ----

/** 用户状态枚举 */
export enum UserStatusEnum {
  /** 正常 */
  Active = 1,
  /** 禁用 */
  Disabled = 0,
}

// ---- 类型定义 ----

/** 用户查询参数 */
export interface UserQueryParams {
  /** 页码（从 1 开始） */
  page?: number
  /** 每页数量 */
  pageSize?: number
  /** 搜索关键字（用户名/昵称） */
  keyword?: string
  /** 用户状态 */
  status?: number
}

/** 角色简要信息 */
export interface RoleBrief {
  /** 角色 ID */
  id: number
  /** 角色编码 */
  code: string
  /** 角色名称 */
  name: string
}

/** 用户列表项 */
export interface UserItem {
  /** 用户 ID */
  id: number
  /** 用户名 */
  userName: string
  /** 昵称 */
  nickName: string
  /** 邮箱 */
  email: string
  /** 状态（1=正常，0=禁用） */
  status: number
  /** 描述 */
  description: string
  /** 角色列表 */
  roles: RoleBrief[]
  /** 创建时间 */
  createTime: string
  /** 更新时间 */
  updateTime: string
}

/** 创建用户参数 */
export interface CreateUserParams {
  /** 用户名 */
  userName: string
  /** 密码 */
  password: string
  /** 昵称 */
  nickName: string
  /** 邮箱（可选） */
  email?: string
  /** 描述（可选） */
  description?: string
  /** 角色 ID 列表（可选） */
  roleIds?: number[]
}

/** 更新用户参数 */
export interface UpdateUserParams {
  /** 用户 ID */
  id: number
  /** 昵称 */
  nickName: string
  /** 邮箱（可选） */
  email?: string
  /** 状态（可选） */
  status?: number
  /** 描述（可选） */
  description?: string
}

// ---- 接口契约（Strategy Interface）----

/** 用户管理 API 接口契约 */
export interface IUserApi {
  /**
   * 获取用户分页列表
   * @param params - 查询参数
   * @returns 分页用户列表
   */
  getUserList(params: UserQueryParams): Promise<PaginatedResponse<UserItem>>
  /**
   * 创建用户
   * @param data - 创建参数
   * @returns 新创建的用户
   */
  createUser(data: CreateUserParams): Promise<UserItem>
  /**
   * 更新用户信息
   * @param data - 更新参数
   * @returns 更新后的用户
   */
  updateUser(data: UpdateUserParams): Promise<UserItem>
  /**
   * 删除用户
   * @param id - 用户 ID
   */
  deleteUser(id: number): Promise<void>
  /**
   * 获取用户角色列表
   * @param userId - 用户 ID
   * @returns 角色简要信息列表
   */
  getUserRoles(userId: number): Promise<RoleBrief[]>
  /**
   * 为用户分配角色
   * @param userId - 用户 ID
   * @param roleIds - 角色 ID 列表
   */
  assignUserRoles(userId: number, roleIds: number[]): Promise<void>
}

// ---- Real 实现（适配 HTTP）----

class RealUserApi implements IUserApi {
  async getUserList(params: UserQueryParams) {
    return request<PaginatedResponse<UserItem>>({
      url: '/api/manager/user/list',
      method: 'POST',
      data: params
    })
  }

  async createUser(data: CreateUserParams) {
    return request<UserItem>({
      url: '/api/manager/user/create',
      method: 'POST',
      data
    })
  }

  async updateUser(data: UpdateUserParams) {
    return request<UserItem>({
      url: '/api/manager/user/update',
      method: 'POST',
      data
    })
  }

  async deleteUser(id: number) {
    await request<void>({
      url: '/api/manager/user/delete',
      method: 'POST',
      data: { id }
    })
  }

  async getUserRoles(userId: number) {
    return request<RoleBrief[]>({
      url: `/api/manager/user/${userId}/roles`,
      method: 'POST'
    })
  }

  async assignUserRoles(userId: number, roleIds: number[]) {
    await request<void>({
      url: `/api/manager/user/${userId}/assign-roles`,
      method: 'POST',
      data: { roleIds }
    })
  }
}

// ---- Mock 实现（适配本地数据）----

class MockUserApi implements IUserApi {
  async getUserList(params: UserQueryParams): Promise<PaginatedResponse<UserItem>> {
    const { page = 1, pageSize = 10, keyword, status } = params

    let filtered = [...mockUserList]

    // 按关键字过滤
    if (keyword) {
      const kw = keyword.toLowerCase()
      filtered = filtered.filter(
        u => u.userName.toLowerCase().includes(kw) || u.nickName.toLowerCase().includes(kw)
      )
    }

    // 按状态过滤
    if (status !== undefined && status !== null) {
      filtered = filtered.filter(u => u.status === status)
    }

    const start = (page - 1) * pageSize
    return {
      list: filtered.slice(start, start + pageSize),
      total: filtered.length,
      page,
      pageSize
    }
  }

  async createUser(data: CreateUserParams): Promise<UserItem> {
    return {
      id: Date.now(),
      userName: data.userName,
      nickName: data.nickName,
      email: data.email ?? '',
      status: UserStatusEnum.Active,
      description: data.description ?? '',
      roles: [],
      createTime: new Date().toISOString(),
      updateTime: new Date().toISOString()
    }
  }

  async updateUser(data: UpdateUserParams): Promise<UserItem> {
    const existing = mockUserList.find(u => u.id === data.id)
    if (!existing) throw new Error(`用户 ${data.id} 不存在`)
    return {
      ...existing,
      nickName: data.nickName,
      email: data.email ?? existing.email,
      status: data.status ?? existing.status,
      description: data.description ?? existing.description,
      updateTime: new Date().toISOString()
    }
  }

  async deleteUser(id: number): Promise<void> {
    const existing = mockUserList.find(u => u.id === id)
    if (!existing) throw new Error(`用户 ${id} 不存在`)
  }

  async getUserRoles(userId: number): Promise<RoleBrief[]> {
    const user = mockUserList.find(u => u.id === userId)
    if (!user) throw new Error(`用户 ${userId} 不存在`)
    return user.roles
  }

  async assignUserRoles(_userId: number, _roleIds: number[]): Promise<void> {
    // Mock 模式下不实际修改数据
  }
}

// ---- 工厂函数（Simple Factory）----

/** 创建用户管理 API 实例 */
export function createUserApi(): IUserApi {
  return MOCK_ENABLED ? new MockUserApi() : new RealUserApi()
}

// ---- 模块实例（模块级单例）----

/** 用户管理 API 单例 */
export const userApi = createUserApi()
