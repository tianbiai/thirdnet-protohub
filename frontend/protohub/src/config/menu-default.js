// 默认菜单配置
export const defaultMenuConfig = {
  title: '项目管理聚合基座',
  version: '1.0.0',
  groups: []
}

// 菜单项类型
export const itemTypes = [
  { value: 'web', label: 'Web 应用', icon: '🖥️' },
  { value: 'mobile', label: '移动端应用', icon: '📱' },
  { value: 'doc', label: '文档', icon: '📋' },
  { value: 'swagger', label: 'API 文档', icon: '📚' },
  { value: 'internal', label: '内部页面', icon: '🔗' }
]

// 默认视口配置
export const defaultViewport = {
  mobile: { width: 375, height: 812 },
  tablet: { width: 768, height: 1024 }
}

// 生成唯一 ID
export function generateId(prefix = 'item') {
  return `${prefix}-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`
}
