/**
 * 菜单相关工具函数
 */

// 菜单项类型配置
export const ITEM_TYPES = {
  web: {
    label: 'Web 应用',
    shortLabel: 'Web',
    type: '',
    icon: 'Web',
    emoji: '🖥️'
  },
  miniprogram: {
    label: '小程序',
    shortLabel: '小程序',
    type: 'success',
    icon: 'Iphone',
    emoji: '📱'
  },
  doc: {
    label: '文档',
    shortLabel: '文档',
    type: 'info',
    icon: 'Document',
    emoji: '📄'
  },
  swagger: {
    label: 'API 文档',
    shortLabel: 'API',
    type: 'danger',
    icon: 'DataBoard',
    emoji: '📚'
  },
  internal: {
    label: '内部页面',
    shortLabel: '内部',
    type: 'warning',
    icon: 'Link',
    emoji: '🔗'
  }
}

// 类型选项列表（用于下拉选择）
export const TYPE_OPTIONS = [
  { value: 'web', label: 'Web 应用', icon: '🖥️' },
  { value: 'miniprogram', label: '小程序', icon: '📱' },
  { value: 'doc', label: '文档', icon: '📄' },
  { value: 'swagger', label: 'API 文档', icon: '📚' }
]

// 默认视口配置
export const DEFAULT_VIEWPORT = { width: 375, height: 812 }

/**
 * 获取类型标签信息
 * @param {string} type - 菜单项类型
 * @returns {Object} 包含 label, type(用于 el-tag), icon 的对象
 */
export function getTypeTag(type) {
  const typeConfig = ITEM_TYPES[type] || ITEM_TYPES.web
  return {
    label: typeConfig.shortLabel,
    type: typeConfig.type,
    icon: typeConfig.emoji
  }
}

/**
 * 获取类型完整信息
 * @param {string} type - 菜单项类型
 * @returns {Object} 完整的类型配置
 */
export function getTypeConfig(type) {
  return ITEM_TYPES[type] || ITEM_TYPES.web
}

/**
 * 创建空的菜单项对象
 * @returns {Object} 空的菜单项配置
 */
export function createEmptyMenuItem() {
  return {
    name: '',
    icon: '',
    type: 'web',
    url: '',
    route: '',
    description: '',
    apiDescription: '',
    docDescription: '',
    docFileId: '',
    docFileName: '',
    useAuth: false,
    authUsername: '',
    authPassword: '',
    viewport: { ...DEFAULT_VIEWPORT }
  }
}

/**
 * 创建空的分组对象
 * @param {string} name - 分组名称
 * @param {string} icon - 分组图标
 * @returns {Object} 空的分组配置
 */
export function createEmptyGroup(name = '新分组', icon = '📁') {
  return {
    name,
    icon,
    expanded: true,
    children: []
  }
}

/**
 * 重置表单数据到默认值
 * @param {Object} formData - 表单数据对象
 */
export function resetFormData(formData) {
  const defaults = createEmptyMenuItem()
  Object.keys(defaults).forEach(key => {
    if (key === 'viewport') {
      formData[key] = { ...DEFAULT_VIEWPORT }
    } else {
      formData[key] = defaults[key]
    }
  })
}

export default {
  ITEM_TYPES,
  TYPE_OPTIONS,
  DEFAULT_VIEWPORT,
  getTypeTag,
  getTypeConfig,
  createEmptyMenuItem,
  createEmptyGroup,
  resetFormData
}
