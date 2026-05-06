/**
 * 菜单管理 Mock 数据
 *
 * 提供菜单分组和菜单项的模拟数据
 */

import type { MenuGroupItem, MenuItemDetail } from '@/api/modules/manager/menu'

/** 菜单分组列表 Mock 数据 */
export const mockMenuGroupList: MenuGroupItem[] = [
  {
    id: 1,
    name: '内部工具',
    icon: 'Tools',
    order: 1,
    itemCount: 3
  },
  {
    id: 2,
    name: '项目管理',
    icon: 'Folder',
    order: 2,
    itemCount: 2
  },
  {
    id: 3,
    name: '文档中心',
    icon: 'Document',
    order: 3,
    itemCount: 2
  },
  {
    id: 4,
    name: '移动应用',
    icon: 'Iphone',
    order: 4,
    itemCount: 1
  }
]

/** 菜单项列表 Mock 数据 */
export const mockMenuItemList: MenuItemDetail[] = [
  // ---- 内部工具（groupId: 1）----
  {
    id: 101,
    groupId: 1,
    name: 'ProtoPick 书签工具',
    type: 'web',
    url: 'https://protopick.protohub.com',
    description: '原型快速拾取书签工具，用于从网页中提取设计元素',
    order: 1,
    viewport: null,
    route: ''
  },
  {
    id: 102,
    groupId: 1,
    name: '接口调试平台',
    type: 'web',
    url: 'https://api-debug.protohub.com',
    description: '统一接口调试和文档查看平台',
    order: 2,
    viewport: null,
    route: ''
  },
  {
    id: 103,
    groupId: 1,
    name: '系统监控面板',
    type: 'web',
    url: 'https://monitor.protohub.com',
    description: '服务器状态和应用性能实时监控',
    order: 3,
    viewport: null,
    route: ''
  },
  // ---- 项目管理（groupId: 2）----
  {
    id: 201,
    groupId: 2,
    name: '需求管理',
    type: 'web',
    url: 'https://req.protohub.com',
    description: '产品需求收集、评审和跟踪管理',
    order: 1,
    viewport: null,
    route: ''
  },
  {
    id: 202,
    groupId: 2,
    name: '迭代看板',
    type: 'web',
    url: 'https://board.protohub.com',
    description: '敏捷迭代任务看板，支持拖拽排序',
    order: 2,
    viewport: null,
    route: ''
  },
  // ---- 文档中心（groupId: 3）----
  {
    id: 301,
    groupId: 3,
    name: '开发规范文档',
    type: 'doc',
    url: '',
    description: '前端和后端开发规范，包含代码风格和提交规范',
    order: 1,
    viewport: null,
    route: '/doc/dev-guide'
  },
  {
    id: 302,
    groupId: 3,
    name: 'API 接口文档',
    type: 'doc',
    url: '',
    description: 'RESTful API 接口文档，含请求示例和错误码说明',
    order: 2,
    viewport: null,
    route: '/doc/api-reference'
  },
  // ---- 移动应用（groupId: 4）----
  {
    id: 401,
    groupId: 4,
    name: '移动端原型预览',
    type: 'miniprogram',
    url: 'https://mobile.protohub.com/preview',
    description: '移动端原型在线预览，支持 iPhone 和 Android 视口',
    order: 1,
    viewport: { width: 375, height: 812 },
    route: ''
  }
]
