/**
 * 菜单模块 Mock 数据
 *
 * 提供菜单配置、菜单分组和菜单项的模拟数据。
 */

import type {
  MenuConfigResponse,
  MenuGroup,
  MenuItem
} from '@/api/modules/app/menu'
import { MenuItemType } from '@/api/modules/app/menu'

// ==================== 菜单项数据 ====================

/** 模拟内部系统菜单项 */
export const mockInternalItems: MenuItem[] = [
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
  },
  {
    id: 103,
    name: '系统监控面板',
    type: MenuItemType.Web,
    url: 'https://monitor.protohub.example.com',
    description: '服务器运行状态监控',
    order: 3,
    viewport: null,
    route: ''
  }
]

/** 模拟移动端菜单项 */
export const mockMobileItems: MenuItem[] = [
  {
    id: 201,
    name: '商城小程序',
    type: MenuItemType.Miniprogram,
    url: 'https://shop.protohub.example.com',
    description: '移动端商城应用原型',
    order: 1,
    viewport: { width: 375, height: 812 },
    route: ''
  },
  {
    id: 202,
    name: '办公助手',
    type: MenuItemType.Miniprogram,
    url: 'https://office.protohub.example.com',
    description: '移动办公应用原型',
    order: 2,
    viewport: { width: 375, height: 667 },
    route: ''
  },
  {
    id: 203,
    name: '社交应用',
    type: MenuItemType.Miniprogram,
    url: 'https://social.protohub.example.com',
    description: '社交平台应用原型',
    order: 3,
    viewport: { width: 375, height: 812 },
    route: ''
  }
]

/** 模拟外部链接菜单项 */
export const mockLinkItems: MenuItem[] = [
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

/** 模拟内部页面菜单项 */
export const mockInternalRouteItems: MenuItem[] = [
  {
    id: 401,
    name: '使用指南',
    type: MenuItemType.Internal,
    url: '',
    description: 'ProtoHub 使用指南',
    order: 1,
    viewport: null,
    route: '/guide'
  },
  {
    id: 402,
    name: '更新日志',
    type: MenuItemType.Internal,
    url: '',
    description: '版本更新记录',
    order: 2,
    viewport: null,
    route: '/changelog'
  }
]

// ==================== 菜单分组数据 ====================

/** 模拟菜单分组列表 */
export const mockMenuGroups: MenuGroup[] = [
  {
    id: 1,
    name: '内部系统',
    icon: 'Monitor',
    order: 1,
    children: mockInternalItems
  },
  {
    id: 2,
    name: '移动端应用',
    icon: 'Iphone',
    order: 2,
    children: mockMobileItems
  },
  {
    id: 3,
    name: '外部链接',
    icon: 'Link',
    order: 3,
    children: mockLinkItems
  },
  {
    id: 4,
    name: '帮助文档',
    icon: 'Document',
    order: 4,
    children: mockInternalRouteItems
  }
]

// ==================== 菜单配置数据 ====================

/** 模拟完整菜单配置 */
export const mockMenuConfig: MenuConfigResponse = {
  title: 'ProtoHub 原型视界',
  version: '1.0.0',
  groups: mockMenuGroups
}
