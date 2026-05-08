/**
 * 菜单模块 API（用户端）
 *
 * 提供获取菜单配置等接口，返回菜单分组及菜单项的树形结构。
 */

import { request } from '@/api/request'
import { MOCK_ENABLED } from '@/config'

// ==================== 枚举 ====================

/** 菜单项类型枚举 */
export enum MenuItemType {
  /** Web 应用（iframe 显示） */
  Web = 'web',
  /** 小程序/移动端（自定义视口） */
  Miniprogram = 'miniprogram',
  /** 超链接（外部链接） */
  Link = 'link',
  /** Markdown 文档 */
  Doc = 'doc',
  /** 内部路由 */
  Internal = 'internal'
}

// ==================== 类型定义 ====================

/** 视口配置 */
export interface Viewport {
  /** 视口宽度 */
  width: number
  /** 视口高度 */
  height: number
}

/** 菜单项 */
export interface MenuItem {
  /** 菜单项 ID */
  id: number
  /** 菜单名称 */
  name: string
  /** 菜单类型（web/miniprogram/link/doc/internal） */
  type: string
  /** 访问地址 */
  url: string
  /** 菜单描述 */
  description: string
  /** 排序序号 */
  order: number
  /** 视口配置（小程序类型使用） */
  viewport: Viewport | null
  /** 内部路由路径 */
  route: string
}

/** 菜单分组 */
export interface MenuGroup {
  /** 分组 ID */
  id: number
  /** 分组名称 */
  name: string
  /** 分组图标 */
  icon: string
  /** 排序序号 */
  order: number
  /** 分组下的菜单项（兼容字段） */
  items?: MenuItem[]
  /** 分组下的菜单项 */
  children?: MenuItem[]
}

/** 菜单配置响应 */
export interface MenuConfigResponse {
  /** 菜单标题 */
  title: string
  /** 版本号 */
  version: string
  /** 菜单分组列表 */
  groups: MenuGroup[]
}

// ==================== 接口定义 ====================

/** 菜单模块接口 */
export interface IMenuApi {
  /**
   * 获取菜单配置
   * 返回完整的菜单结构，包含分组和菜单项
   * @returns 菜单配置
   */
  getMenuConfig(): Promise<MenuConfigResponse>
}

// ==================== Real 实现 ====================

/** 菜单模块 - 真实 API 实现 */
export class RealMenuApi implements IMenuApi {
  /**
   * 获取菜单配置
   */
  async getMenuConfig(): Promise<MenuConfigResponse> {
    return request<MenuConfigResponse>({
      url: '/api/app/menu/list',
      method: 'POST'
    })
  }
}

// ==================== Mock 实现 ====================

/** 菜单模块 - Mock 实现 */
export class MockMenuApi implements IMenuApi {
  /**
   * 模拟获取菜单配置
   */
  async getMenuConfig(): Promise<MenuConfigResponse> {
    await new Promise(resolve => setTimeout(resolve, 300))

    return {
      title: 'ProtoHub 原型视界',
      version: '1.0.0',
      groups: [
        {
          id: 1,
          name: '内部系统',
          icon: 'Monitor',
          order: 1,
          children: [
            {
              id: 101,
              name: 'ProtoHub 管理后台',
              type: MenuItemType.Web,
              url: 'https://admin.protohub.example.com',
              description: '项目管理系统后台界面',
              order: 1,
              viewport: null,
              route: ''
            },
            {
              id: 102,
              name: 'API 文档中心',
              type: MenuItemType.Doc,
              url: '',
              description: '后端接口 API 文档',
              order: 2,
              viewport: null,
              route: '/doc/api'
            }
          ]
        },
        {
          id: 2,
          name: '移动端应用',
          icon: 'Iphone',
          order: 2,
          children: [
            {
              id: 201,
              name: 'GAS 小程序',
              type: MenuItemType.Miniprogram,
              url: 'http://61.164.57.60:9876/gasminiprogram/#/',
              description: 'GAS 移动端应用',
              order: 1,
              viewport: null,
              route: ''
            },
            {
              id: 202,
              name: '商城小程序',
              type: MenuItemType.Miniprogram,
              url: 'https://shop.protohub.example.com',
              description: '移动端商城应用原型',
              order: 2,
              viewport: { width: 375, height: 812 },
              route: ''
            },
            {
              id: 203,
              name: '办公助手',
              type: MenuItemType.Miniprogram,
              url: 'https://office.protohub.example.com',
              description: '移动办公应用原型',
              order: 3,
              viewport: { width: 375, height: 667 },
              route: ''
            }
          ]
        },
        {
          id: 3,
          name: '外部链接',
          icon: 'Link',
          order: 3,
          children: [
            {
              id: 301,
              name: '项目仓库',
              type: MenuItemType.Link,
              url: 'https://git.example.com/protohub',
              description: 'Git 代码仓库',
              order: 1,
              viewport: null,
              route: ''
            },
            {
              id: 302,
              name: '设计稿',
              type: MenuItemType.Link,
              url: 'https://figma.example.com/protohub',
              description: 'Figma 设计稿',
              order: 2,
              viewport: null,
              route: ''
            }
          ]
        }
      ]
    }
  }
}

// ==================== 工厂函数 ====================

/**
 * 创建菜单 API 实例
 * 根据全局 Mock 开关返回真实或模拟实现
 * @returns 菜单 API 实例
 */
export function createMenuApi(): IMenuApi {
  return MOCK_ENABLED ? new MockMenuApi() : new RealMenuApi()
}

// ==================== 单例导出 ====================

/** 菜单模块 API 单例 */
export const menuApi = createMenuApi()
