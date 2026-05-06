/**
 * 权限查询 Mock 数据
 *
 * 提供权限列表和分类统计的模拟数据
 */

import type { PermissionItem, PermissionCategory } from '@/api/modules/manager/permission'

/** 权限列表 Mock 数据 */
export const mockPermissionList: PermissionItem[] = [
  {
    id: 1,
    code: 'user:manage',
    name: '用户管理',
    category: '用户',
    description: '管理用户的创建、编辑、删除和状态变更'
  },
  {
    id: 2,
    code: 'user:assign_role',
    name: '用户角色分配',
    category: '用户',
    description: '为用户分配或移除角色'
  },
  {
    id: 3,
    code: 'role:manage',
    name: '角色管理',
    category: '角色',
    description: '管理角色的创建、编辑和删除'
  },
  {
    id: 4,
    code: 'role:assign_permission',
    name: '角色权限分配',
    category: '角色',
    description: '为角色分配或移除权限'
  },
  {
    id: 5,
    code: 'menu:manage',
    name: '菜单管理',
    category: '菜单',
    description: '管理菜单分组和菜单项的增删改查'
  },
  {
    id: 6,
    code: 'menu:publish',
    name: '菜单发布',
    category: '菜单',
    description: '发布菜单配置到线上环境'
  },
  {
    id: 7,
    code: 'system:config',
    name: '系统配置',
    category: '系统',
    description: '修改系统级别的全局配置参数'
  },
  {
    id: 8,
    code: 'system:log',
    name: '系统日志',
    category: '系统',
    description: '查看系统操作日志和审计记录'
  },
  {
    id: 9,
    code: 'project:manage',
    name: '项目管理',
    category: '项目',
    description: '管理项目的创建、编辑和删除'
  },
  {
    id: 10,
    code: 'project:access',
    name: '项目授权',
    category: '项目',
    description: '管理项目访问权限的授予和撤销'
  }
]

/** 权限分类统计 Mock 数据 */
export const mockPermissionCategories: PermissionCategory[] = [
  { name: '用户', count: 2 },
  { name: '角色', count: 2 },
  { name: '菜单', count: 2 },
  { name: '系统', count: 2 },
  { name: '项目', count: 2 }
]
