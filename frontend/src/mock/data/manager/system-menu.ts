/**
 * 系统菜单管理 Mock 数据
 *
 * 提供系统菜单的扁平列表和树形结构模拟数据
 */

import type { SystemMenuItem } from '@/api/modules/manager/system-menu'

/** 系统菜单扁平列表 Mock 数据 */
export const mockSystemMenuList: SystemMenuItem[] = [
  {
    id: 1,
    name: '首页',
    path: '/dashboard',
    icon: 'HomeFilled',
    parentId: null,
    order: 1,
    permission: '',
    children: []
  },
  {
    id: 2,
    name: '项目管理',
    path: '/projects',
    icon: 'Folder',
    parentId: null,
    order: 2,
    permission: '',
    children: [
      {
        id: 21,
        name: '项目列表',
        path: '/projects/list',
        icon: '',
        parentId: 2,
        order: 1,
        permission: 'project:view',
        children: []
      },
      {
        id: 22,
        name: '项目配置',
        path: '/projects/config',
        icon: '',
        parentId: 2,
        order: 2,
        permission: 'project:manage',
        children: []
      }
    ]
  },
  {
    id: 3,
    name: '系统管理',
    path: '/system',
    icon: 'Setting',
    parentId: null,
    order: 3,
    permission: '',
    children: [
      {
        id: 31,
        name: '用户管理',
        path: '/system/user',
        icon: '',
        parentId: 3,
        order: 1,
        permission: 'user:manage',
        children: []
      },
      {
        id: 32,
        name: '角色管理',
        path: '/system/role',
        icon: '',
        parentId: 3,
        order: 2,
        permission: 'role:manage',
        children: []
      },
      {
        id: 33,
        name: '菜单管理',
        path: '/system/menu',
        icon: '',
        parentId: 3,
        order: 3,
        permission: 'menu:manage',
        children: []
      },
      {
        id: 34,
        name: '权限管理',
        path: '/system/permission',
        icon: '',
        parentId: 3,
        order: 4,
        permission: 'system:config',
        children: []
      }
    ]
  },
  {
    id: 4,
    name: '项目授权',
    path: '/access',
    icon: 'Lock',
    parentId: null,
    order: 4,
    permission: 'project:access',
    children: []
  }
]

/** 系统菜单树形结构 Mock 数据（与扁平列表同构） */
export const mockSystemMenuTree: SystemMenuItem[] = mockSystemMenuList
