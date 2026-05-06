/**
 * 用户管理 Mock 数据
 *
 * 提供用户列表的模拟数据，包含中文姓名和描述
 */

import type { UserItem } from '@/api/modules/manager/user'

/** 用户列表 Mock 数据 */
export const mockUserList: UserItem[] = [
  {
    id: 1,
    userName: 'admin',
    nickName: '超级管理员',
    email: 'admin@protohub.com',
    status: 1,
    description: '系统初始管理员账号，拥有全部权限',
    roles: [
      { id: 1, code: 'admin', name: '管理员' }
    ],
    createTime: '2024-01-01T00:00:00',
    updateTime: '2024-01-01T00:00:00'
  },
  {
    id: 2,
    userName: 'zhangsan',
    nickName: '张三',
    email: 'zhangsan@example.com',
    status: 1,
    description: '产品经理，负责原型设计模块',
    roles: [
      { id: 2, code: 'editor', name: '编辑者' },
      { id: 3, code: 'viewer', name: '查看者' }
    ],
    createTime: '2024-03-15T10:30:00',
    updateTime: '2024-06-20T14:22:00'
  },
  {
    id: 3,
    userName: 'lisi',
    nickName: '李四',
    email: 'lisi@example.com',
    status: 1,
    description: '前端开发工程师',
    roles: [
      { id: 3, code: 'viewer', name: '查看者' }
    ],
    createTime: '2024-04-08T09:15:00',
    updateTime: '2024-07-01T16:45:00'
  },
  {
    id: 4,
    userName: 'wangwu',
    nickName: '王五',
    email: 'wangwu@example.com',
    status: 0,
    description: '已离职，账号已禁用',
    roles: [],
    createTime: '2024-02-20T11:00:00',
    updateTime: '2024-08-10T08:30:00'
  },
  {
    id: 5,
    userName: 'zhaoliu',
    nickName: '赵六',
    email: 'zhaoliu@example.com',
    status: 1,
    description: '项目经理，负责进度跟踪和需求管理',
    roles: [
      { id: 2, code: 'editor', name: '编辑者' }
    ],
    createTime: '2024-05-12T13:40:00',
    updateTime: '2024-09-05T10:10:00'
  }
]
