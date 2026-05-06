/**
 * 项目访问授权管理 Mock 数据
 *
 * 提供项目访问授权列表和用户项目列表的模拟数据
 */

import type { ProjectAccessItem, UserProjectItem } from '@/api/modules/manager/project-access'

/** 项目访问授权列表 Mock 数据 */
export const mockAccessList: ProjectAccessItem[] = [
  {
    id: 1,
    userId: 2,
    userName: 'zhangsan',
    userNickName: '张三',
    projectId: 1,
    projectName: 'ProtoHub 原型平台',
    accessType: 'manage',
    grantedBy: 1,
    grantedByName: '超级管理员',
    createTime: '2024-03-20T10:00:00'
  },
  {
    id: 2,
    userId: 3,
    userName: 'lisi',
    userNickName: '李四',
    projectId: 1,
    projectName: 'ProtoHub 原型平台',
    accessType: 'view',
    grantedBy: 1,
    grantedByName: '超级管理员',
    createTime: '2024-04-15T14:30:00'
  },
  {
    id: 3,
    userId: 5,
    userName: 'zhaoliu',
    userNickName: '赵六',
    projectId: 2,
    projectName: '数据看板系统',
    accessType: 'manage',
    grantedBy: 1,
    grantedByName: '超级管理员',
    createTime: '2024-05-10T09:00:00'
  },
  {
    id: 4,
    userId: 2,
    userName: 'zhangsan',
    userNickName: '张三',
    projectId: 2,
    projectName: '数据看板系统',
    accessType: 'view',
    grantedBy: 5,
    grantedByName: '赵六',
    createTime: '2024-05-12T11:20:00'
  },
  {
    id: 5,
    userId: 3,
    userName: 'lisi',
    userNickName: '李四',
    projectId: 3,
    projectName: '移动端商城',
    accessType: 'view',
    grantedBy: 1,
    grantedByName: '超级管理员',
    createTime: '2024-06-01T16:45:00'
  }
]

/** 用户项目访问列表 Mock 数据 */
export const mockUserProjects: UserProjectItem[] = [
  {
    id: 1,
    projectId: 1,
    projectName: 'ProtoHub 原型平台',
    projectIcon: 'Platform',
    accessType: 'manage',
    createTime: '2024-03-20T10:00:00'
  },
  {
    id: 2,
    projectId: 2,
    projectName: '数据看板系统',
    projectIcon: 'DataAnalysis',
    accessType: 'manage',
    createTime: '2024-05-10T09:00:00'
  },
  {
    id: 3,
    projectId: 3,
    projectName: '移动端商城',
    projectIcon: 'ShoppingCart',
    accessType: 'view',
    createTime: '2024-06-01T16:45:00'
  }
]
