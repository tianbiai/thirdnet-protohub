/**
 * 菜单相关工具函数
 */

/**
 * 给 URL 追加时间戳参数，避免浏览器缓存
 * @param {string} url - 原始 URL
 * {number} key - 时间戳值
 * @returns {string} 带缓存破坏参数的 URL
 */
export function cacheBustUrl(url, key = Date.now()) {
  if (!url) return ''
  const sep = url.includes('?') ? '&' : '?'
  return `${url}${sep}_t=${key}`
}

// 菜单项类型配置
export const ITEM_TYPES = {
  web: {
    label: 'Web 应用',
    shortLabel: 'Web',
    type: '',
    icon: 'Monitor',
    emoji: '🖥️',
    color: '#007AFF',
    bgColor: 'rgba(0, 122, 255, 0.12)',
    badgeText: 'WEB'
  },
  miniprogram: {
    label: '小程序',
    shortLabel: '小程序',
    type: 'success',
    icon: 'Cellphone',
    emoji: '📱',
    color: '#30D158',
    bgColor: 'rgba(48, 209, 88, 0.12)',
    badgeText: '小程序'
  },
  link: {
    label: '超链接',
    shortLabel: '链接',
    type: 'danger',
    icon: 'Link',
    emoji: '🔗',
    color: '#FF453A',
    bgColor: 'rgba(255, 69, 58, 0.12)',
    badgeText: '链接'
  },
  changelog: {
    label: '变更日志',
    shortLabel: '日志',
    type: 'info',
    icon: 'Notebook',
    emoji: '📝',
    color: '#64D2FF',
    bgColor: 'rgba(100, 210, 255, 0.12)',
    badgeText: 'LOG'
  }
}

// 类型选项列表（用于下拉选择）
export const TYPE_OPTIONS = [
  { value: 'web', label: 'Web 应用', icon: '🖥️' },
  { value: 'miniprogram', label: '小程序应用', icon: '📱' },
  { value: 'link', label: '超链接', icon: '🔗' },
  { value: 'changelog', label: '变更日志', icon: '📝' }
]

// 默认视口配置
export const DEFAULT_VIEWPORT = { width: 375, height: 812 }

/**
 * 获取类型标签信息
 * @param {string} type - 菜单项类型
 * @returns {Object} 包含 label, type(用于 el-tag), icon, color 的对象
 */
export function getTypeTag(type) {
  const typeConfig = ITEM_TYPES[type] || ITEM_TYPES.web
  return {
    label: typeConfig.shortLabel,
    type: typeConfig.type,
    icon: typeConfig.emoji,
    iconComponent: typeConfig.icon,
    color: typeConfig.color,
    bgColor: typeConfig.bgColor,
    badgeText: typeConfig.badgeText
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
    type: 'web',
    url: '',
    route: '',
    description: '',
    apiDescription: '',
    docDescription: '',
    useAuth: false,
    authUsername: '',
    authPassword: '',
    viewportConfig: { ...DEFAULT_VIEWPORT }
  }
}

/**
 * 创建空的分组对象
 * @param {string} name - 分组名称
 * @param {string} icon - 分组图标
 * @returns {Object} 空的分组配置
 */
export function createEmptyGroup(name = '新分组') {
  return {
    name,
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
    if (key === 'viewportConfig') {
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
