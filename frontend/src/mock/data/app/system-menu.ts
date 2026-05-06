/**
 * 系统菜单模块 Mock 数据
 *
 * 提供系统导航菜单的模拟数据，包含树形结构的菜单项。
 */

import type { SystemMenuItem } from '@/api/modules/app/system-menu'

// ==================== 系统菜单数据 ====================

/** 模拟系统菜单列表（树形结构） */
export const mockSystemMenuList: SystemMenuItem[] = [
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
      },
      {
        id: 23,
        name: '最近访问',
        path: '/projects/recent',
        icon: 'Clock',
        parentId: 2,
        order: 3,
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
      },
      {
        id: 33,
        name: '常见问题',
        path: '/docs/faq',
        icon: 'QuestionFilled',
        parentId: 3,
        order: 3,
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
      },
      {
        id: 43,
        name: '通知设置',
        path: '/settings/notification',
        icon: 'Bell',
        parentId: 4,
        order: 3,
        children: []
      }
    ]
  }
]
