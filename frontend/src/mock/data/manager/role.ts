/**
 * 角色管理 Mock 数据
 *
 * 提供角色列表和角色详情的模拟数据，包含中文角色名和权限信息
 */

import type { RoleItem, RoleDetail } from '@/api/modules/manager/role'

/** 角色列表 Mock 数据 */
export const mockRoleList: RoleItem[] = [
  {
    id: 1,
    code: 'admin',
    name: '管理员',
    description: '系统管理员，拥有所有功能的操作权限',
    isSystem: true,
    createTime: '2024-01-01T00:00:00',
    updateTime: '2024-01-01T00:00:00'
  },
  {
    id: 2,
    code: 'editor',
    name: '编辑者',
    description: '可以创建和编辑内容，但不能管理用户和系统配置',
    isSystem: false,
    createTime: '2024-02-15T09:00:00',
    updateTime: '2024-05-20T11:30:00'
  },
  {
    id: 3,
    code: 'viewer',
    name: '查看者',
    description: '只读权限，可以查看所有内容但不能修改',
    isSystem: false,
    createTime: '2024-02-15T09:05:00',
    updateTime: '2024-05-20T11:35:00'
  },
  {
    id: 4,
    code: 'project_manager',
    name: '项目经理',
    description: '可以管理项目配置和成员，但不能修改系统设置',
    isSystem: false,
    createTime: '2024-03-10T14:20:00',
    updateTime: '2024-07-15T16:00:00'
  }
]

/** 角色详情 Mock 数据（含权限列表） */
export const mockRoleDetail: RoleDetail[] = [
  {
    id: 1,
    code: 'admin',
    name: '管理员',
    description: '系统管理员，拥有所有功能的操作权限',
    isSystem: true,
    createTime: '2024-01-01T00:00:00',
    updateTime: '2024-01-01T00:00:00',
    permissions: [
      { id: 1, code: 'user:manage', name: '用户管理', category: '用户', description: '管理用户的增删改查' },
      { id: 2, code: 'role:manage', name: '角色管理', category: '角色', description: '管理角色的增删改查' },
      { id: 3, code: 'menu:manage', name: '菜单管理', category: '菜单', description: '管理菜单的增删改查' },
      { id: 4, code: 'system:config', name: '系统配置', category: '系统', description: '修改系统级配置' },
      { id: 5, code: 'project:manage', name: '项目管理', category: '项目', description: '管理项目的增删改查' }
    ]
  },
  {
    id: 2,
    code: 'editor',
    name: '编辑者',
    description: '可以创建和编辑内容，但不能管理用户和系统配置',
    isSystem: false,
    createTime: '2024-02-15T09:00:00',
    updateTime: '2024-05-20T11:30:00',
    permissions: [
      { id: 3, code: 'menu:manage', name: '菜单管理', category: '菜单', description: '管理菜单的增删改查' },
      { id: 5, code: 'project:manage', name: '项目管理', category: '项目', description: '管理项目的增删改查' }
    ]
  },
  {
    id: 3,
    code: 'viewer',
    name: '查看者',
    description: '只读权限，可以查看所有内容但不能修改',
    isSystem: false,
    createTime: '2024-02-15T09:05:00',
    updateTime: '2024-05-20T11:35:00',
    permissions: []
  },
  {
    id: 4,
    code: 'project_manager',
    name: '项目经理',
    description: '可以管理项目配置和成员，但不能修改系统设置',
    isSystem: false,
    createTime: '2024-03-10T14:20:00',
    updateTime: '2024-07-15T16:00:00',
    permissions: [
      { id: 5, code: 'project:manage', name: '项目管理', category: '项目', description: '管理项目的增删改查' },
      { id: 6, code: 'project:access', name: '项目授权', category: '项目', description: '管理项目访问授权' }
    ]
  }
]
