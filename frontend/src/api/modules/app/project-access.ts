/**
 * 项目访问模块 API（用户端）
 *
 * 提供获取当前用户可访问项目列表的接口。
 * 用于首页展示用户有权限的项目卡片。
 */

import { request } from '@/api/request'
import { MOCK_ENABLED } from '@/config'

// ==================== 类型定义 ====================

/** 我的项目项 */
export interface MyProjectItem {
  /** 授权记录 ID */
  id: number
  /** 项目 ID */
  projectId: number
  /** 项目名称 */
  projectName: string
  /** 项目图标 */
  projectIcon: string
  /** 访问类型（full/read/write） */
  accessType: string
  /** 授权时间 */
  createTime: string
}

// ==================== 接口定义 ====================

/** 项目访问模块接口 */
export interface IProjectAccessApi {
  /**
   * 获取当前用户可访问的项目列表
   * 返回当前用户被授权访问的所有项目
   * @returns 项目访问列表
   */
  getMyProjects(): Promise<MyProjectItem[]>
}

// ==================== Real 实现 ====================

/** 项目访问模块 - 真实 API 实现 */
export class RealProjectAccessApi implements IProjectAccessApi {
  /**
   * 获取当前用户可访问的项目列表
   */
  async getMyProjects(): Promise<MyProjectItem[]> {
    return request<MyProjectItem[]>({
      url: '/api/manager/project-access/my-projects',
      method: 'POST'
    })
  }
}

// ==================== Mock 实现 ====================

/** 项目访问模块 - Mock 实现 */
export class MockProjectAccessApi implements IProjectAccessApi {
  /**
   * 模拟获取当前用户可访问的项目列表
   */
  async getMyProjects(): Promise<MyProjectItem[]> {
    await new Promise(resolve => setTimeout(resolve, 300))

    return [
      {
        id: 1,
        projectId: 1,
        projectName: 'ProtoHub 管理平台',
        projectIcon: 'Monitor',
        accessType: 'full',
        createTime: '2025-01-15T10:30:00'
      },
      {
        id: 2,
        projectId: 2,
        projectName: '商城小程序',
        projectIcon: 'ShoppingCart',
        accessType: 'read',
        createTime: '2025-02-20T14:15:00'
      },
      {
        id: 3,
        projectId: 3,
        projectName: '办公助手',
        projectIcon: 'Briefcase',
        accessType: 'write',
        createTime: '2025-03-10T09:00:00'
      },
      {
        id: 4,
        projectId: 4,
        projectName: '数据分析平台',
        projectIcon: 'DataAnalysis',
        accessType: 'full',
        createTime: '2025-04-05T16:45:00'
      }
    ]
  }
}

// ==================== 工厂函数 ====================

/**
 * 创建项目访问 API 实例
 * 根据全局 Mock 开关返回真实或模拟实现
 * @returns 项目访问 API 实例
 */
export function createProjectAccessApi(): IProjectAccessApi {
  return MOCK_ENABLED ? new MockProjectAccessApi() : new RealProjectAccessApi()
}

// ==================== 单例导出 ====================

/** 项目访问模块 API 单例 */
export const projectAccessApi = createProjectAccessApi()
