/**
 * 系统菜单模块 API（用户端）
 *
 * 提供获取当前用户可见系统菜单的接口。
 * 系统菜单用于构建导航栏、侧边栏等系统级菜单结构。
 */

import { request } from '@/api/request'
import { MOCK_ENABLED } from '@/config'

// ==================== 类型定义 ====================

/** 系统菜单项 */
export interface SystemMenuItem {
  /** 菜单项 ID */
  id: number
  /** 菜单名称 */
  name: string
  /** 菜单路径 */
  path: string
  /** 菜单图标 */
  icon: string
  /** 父级菜单 ID（顶级为 null） */
  parentId: number | null
  /** 排序序号 */
  order: number
  /** 子菜单列表 */
  children: SystemMenuItem[]
}

// ==================== 接口定义 ====================

/** 系统菜单模块接口 */
export interface ISystemMenuApi {
  /**
   * 获取当前用户可见的系统菜单
   * 根据用户角色和权限返回对应的菜单树形结构
   * @returns 系统菜单列表（树形结构）
   */
  getMyMenus(): Promise<SystemMenuItem[]>
}

// ==================== Real 实现 ====================

/** 系统菜单模块 - 真实 API 实现 */
export class RealSystemMenuApi implements ISystemMenuApi {
  /**
   * 获取当前用户可见的系统菜单
   */
  async getMyMenus(): Promise<SystemMenuItem[]> {
    return request<SystemMenuItem[]>({
      url: '/api/app/system-menu/my-menus',
      method: 'POST'
    })
  }
}

// ==================== Mock 实现 ====================

/** 系统菜单模块 - Mock 实现 */
export class MockSystemMenuApi implements ISystemMenuApi {
  /**
   * 模拟获取当前用户可见的系统菜单
   */
  async getMyMenus(): Promise<SystemMenuItem[]> {
    await new Promise(resolve => setTimeout(resolve, 300))

    return [
      {
        id: 1,
        name: '首页',
        path: '/home',
        icon: 'HomeFilled',
        parentId: null,
        order: 1,
        children: []
      },
      {
        id: 2,
        name: '项目管理',
        path: '/projects',
        icon: 'Folder',
        parentId: null,
        order: 2,
        children: [
          {
            id: 21,
            name: '项目列表',
            path: '/projects/list',
            icon: 'List',
            parentId: 2,
            order: 1,
            children: []
          },
          {
            id: 22,
            name: '项目收藏',
            path: '/projects/favorites',
            icon: 'Star',
            parentId: 2,
            order: 2,
            children: []
          }
        ]
      },
      {
        id: 3,
        name: '文档中心',
        path: '/docs',
        icon: 'Document',
        parentId: null,
        order: 3,
        children: [
          {
            id: 31,
            name: 'API 文档',
            path: '/docs/api',
            icon: 'Notebook',
            parentId: 3,
            order: 1,
            children: []
          },
          {
            id: 32,
            name: '开发指南',
            path: '/docs/guide',
            icon: 'Reading',
            parentId: 3,
            order: 2,
            children: []
          }
        ]
      },
      {
        id: 4,
        name: '系统设置',
        path: '/settings',
        icon: 'Setting',
        parentId: null,
        order: 4,
        children: [
          {
            id: 41,
            name: '个人设置',
            path: '/settings/profile',
            icon: 'User',
            parentId: 4,
            order: 1,
            children: []
          },
          {
            id: 42,
            name: '主题设置',
            path: '/settings/theme',
            icon: 'Brush',
            parentId: 4,
            order: 2,
            children: []
          }
        ]
      }
    ]
  }
}

// ==================== 工厂函数 ====================

/**
 * 创建系统菜单 API 实例
 * 根据全局 Mock 开关返回真实或模拟实现
 * @returns 系统菜单 API 实例
 */
export function createSystemMenuApi(): ISystemMenuApi {
  return MOCK_ENABLED ? new MockSystemMenuApi() : new RealSystemMenuApi()
}

// ==================== 单例导出 ====================

/** 系统菜单模块 API 单例 */
export const systemMenuApi = createSystemMenuApi()
