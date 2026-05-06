/**
 * 项目访问授权管理 API 模块
 *
 * 提供管理端项目访问授权的查询、授予、撤销等接口
 * 策略工厂模式：Real 调用后端 API，Mock 使用本地模拟数据
 */

import { request } from '@/api/request'
import { MOCK_ENABLED } from '@/config'
import type { PaginatedResponse } from '@/api/types/common'
import { mockAccessList, mockUserProjects } from '@/mock/data/manager/project-access'

// ---- 类型定义 ----

/** 项目访问授权查询参数 */
export interface AccessQueryParams {
  /** 页码（从 1 开始） */
  page?: number
  /** 每页数量 */
  pageSize?: number
  /** 项目 ID（可选） */
  projectId?: number
  /** 用户 ID（可选） */
  userId?: number
  /** 访问类型（可选） */
  accessType?: string
}

/** 项目访问授权项 */
export interface ProjectAccessItem {
  /** 授权记录 ID */
  id: number
  /** 用户 ID */
  userId: number
  /** 用户名 */
  userName: string
  /** 用户昵称 */
  userNickName: string
  /** 项目 ID */
  projectId: number
  /** 项目名称 */
  projectName: string
  /** 访问类型 */
  accessType: string
  /** 授权人 ID（系统自动授权时为 null） */
  grantedBy: number | null
  /** 授权人名称 */
  grantedByName: string
  /** 授权时间 */
  createTime: string
}

/** 授予项目访问权限参数 */
export interface GrantAccessParams {
  /** 用户 ID */
  userId: number
  /** 项目 ID */
  projectId: number
  /** 访问类型 */
  accessType: string
}

/** 用户项目访问项 */
export interface UserProjectItem {
  /** 授权记录 ID */
  id: number
  /** 项目 ID */
  projectId: number
  /** 项目名称 */
  projectName: string
  /** 项目图标 */
  projectIcon: string
  /** 访问类型 */
  accessType: string
  /** 授权时间 */
  createTime: string
}

// ---- 接口契约（Strategy Interface）----

/** 项目访问授权管理 API 接口契约 */
export interface IProjectAccessApi {
  /**
   * 获取项目访问授权分页列表
   * @param params - 查询参数
   * @returns 分页授权列表
   */
  getAccessList(params: AccessQueryParams): Promise<PaginatedResponse<ProjectAccessItem>>
  /**
   * 授予用户项目访问权限
   * @param data - 授权参数
   * @returns 新创建的授权记录
   */
  grantAccess(data: GrantAccessParams): Promise<ProjectAccessItem>
  /**
   * 撤销项目访问权限
   * @param id - 授权记录 ID
   */
  revokeAccess(id: number): Promise<void>
  /**
   * 获取用户已授权的项目列表
   * @param userId - 用户 ID
   * @returns 用户项目访问列表
   */
  getUserProjects(userId: number): Promise<UserProjectItem[]>
}

// ---- Real 实现（适配 HTTP）----

class RealProjectAccessApi implements IProjectAccessApi {
  async getAccessList(params: AccessQueryParams) {
    return request<PaginatedResponse<ProjectAccessItem>>({
      url: '/api/manager/project-access/list',
      method: 'POST',
      data: params
    })
  }

  async grantAccess(data: GrantAccessParams) {
    return request<ProjectAccessItem>({
      url: '/api/manager/project-access/grant',
      method: 'POST',
      data
    })
  }

  async revokeAccess(id: number) {
    await request<void>({
      url: '/api/manager/project-access/revoke',
      method: 'POST',
      data: { id }
    })
  }

  async getUserProjects(userId: number) {
    return request<UserProjectItem[]>({
      url: `/api/manager/project-access/user/${userId}/projects`,
      method: 'POST'
    })
  }
}

// ---- Mock 实现（适配本地数据）----

class MockProjectAccessApi implements IProjectAccessApi {
  async getAccessList(params: AccessQueryParams): Promise<PaginatedResponse<ProjectAccessItem>> {
    const { page = 1, pageSize = 10, projectId, userId, accessType } = params

    let filtered = [...mockAccessList]

    if (projectId) {
      filtered = filtered.filter(a => a.projectId === projectId)
    }
    if (userId) {
      filtered = filtered.filter(a => a.userId === userId)
    }
    if (accessType) {
      filtered = filtered.filter(a => a.accessType === accessType)
    }

    const start = (page - 1) * pageSize
    return {
      list: filtered.slice(start, start + pageSize),
      total: filtered.length,
      page,
      pageSize
    }
  }

  async grantAccess(data: GrantAccessParams): Promise<ProjectAccessItem> {
    return {
      id: Date.now(),
      userId: data.userId,
      userName: '新用户',
      userNickName: '新用户',
      projectId: data.projectId,
      projectName: '新项目',
      accessType: data.accessType,
      grantedBy: 1,
      grantedByName: '管理员',
      createTime: new Date().toISOString()
    }
  }

  async revokeAccess(id: number): Promise<void> {
    const existing = mockAccessList.find(a => a.id === id)
    if (!existing) throw new Error(`授权记录 ${id} 不存在`)
  }

  async getUserProjects(userId: number): Promise<UserProjectItem[]> {
    if (!userId) return []
    return mockUserProjects
  }
}

// ---- 工厂函数（Simple Factory）----

/** 创建项目访问授权管理 API 实例 */
export function createProjectAccessApi(): IProjectAccessApi {
  return MOCK_ENABLED ? new MockProjectAccessApi() : new RealProjectAccessApi()
}

// ---- 模块实例（模块级单例）----

/** 项目访问授权管理 API 单例 */
export const projectAccessApi = createProjectAccessApi()
