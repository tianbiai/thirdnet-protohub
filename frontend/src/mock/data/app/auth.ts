/**
 * 认证模块 Mock 数据
 *
 * 提供用户登录、用户信息等模拟数据。
 */

import type {
  CurrentUserResponse,
  RoleInfo,
  PermissionInfo,
  ProjectAccessInfo
} from '@/api/modules/app/auth'
import { UserRole } from '@/api/modules/app/auth'

// ==================== 角色数据 ====================

/** 模拟角色列表 */
export const mockRoles: RoleInfo[] = [
  { code: UserRole.Admin, name: '管理员' },
  { code: UserRole.Guest, name: '访客' }
]

// ==================== 权限数据 ====================

/** 模拟管理员权限列表 */
export const mockAdminPermissions: PermissionInfo[] = [
  { code: 'menu:manage' },
  { code: 'user:manage' },
  { code: 'role:manage' },
  { code: 'project:manage' },
  { code: 'project:access' },
  { code: 'system:config' }
]

/** 模拟访客权限列表 */
export const mockGuestPermissions: PermissionInfo[] = [
  { code: 'menu:view' },
  { code: 'project:view' }
]

// ==================== 项目访问数据 ====================

/** 模拟项目访问列表 */
export const mockProjectAccessList: ProjectAccessInfo[] = [
  { id: 1, accessType: 'full' },
  { id: 2, accessType: 'read' },
  { id: 3, accessType: 'write' }
]

// ==================== 用户数据 ====================

/** 模拟管理员用户详情 */
export const mockAdminUser: CurrentUserResponse = {
  id: 1,
  userName: 'admin',
  nickName: '系统管理员',
  roles: [
    { code: UserRole.Admin, name: '管理员' }
  ],
  permissions: mockAdminPermissions,
  projects: mockProjectAccessList
}

/** 模拟访客用户详情 */
export const mockGuestUser: CurrentUserResponse = {
  id: 2,
  userName: 'guest',
  nickName: '普通访客',
  roles: [
    { code: UserRole.Guest, name: '访客' }
  ],
  permissions: mockGuestPermissions,
  projects: [
    { id: 1, accessType: 'read' }
  ]
}
