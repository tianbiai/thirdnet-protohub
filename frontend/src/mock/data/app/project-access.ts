/**
 * 项目访问模块 Mock 数据
 *
 * 提供用户可访问项目的模拟数据。
 */

import type { MyProjectItem } from '@/api/modules/app/project-access'

// ==================== 项目访问数据 ====================

/** 模拟用户可访问项目列表 */
export const mockMyProjectList: MyProjectItem[] = [
  {
    id: 1,
    projectId: 1,
    projectName: 'ProtoHub 管理平台',
    projectIcon: 'Monitor',
    accessType: 'full',
    createTime: '2025-01-15T10:30:00'
  },
  {
    id: 2,
    projectId: 2,
    projectName: '商城小程序',
    projectIcon: 'ShoppingCart',
    accessType: 'read',
    createTime: '2025-02-20T14:15:00'
  },
  {
    id: 3,
    projectId: 3,
    projectName: '办公助手',
    projectIcon: 'Briefcase',
    accessType: 'write',
    createTime: '2025-03-10T09:00:00'
  },
  {
    id: 4,
    projectId: 4,
    projectName: '数据分析平台',
    projectIcon: 'DataAnalysis',
    accessType: 'full',
    createTime: '2025-04-05T16:45:00'
  },
  {
    id: 5,
    projectId: 5,
    projectName: '客户关系管理系统',
    projectIcon: 'UserFilled',
    accessType: 'read',
    createTime: '2025-04-18T11:20:00'
  }
]
