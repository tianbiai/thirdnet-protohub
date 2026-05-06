/**
 * 菜单相关工具函数
 */

/** 菜单项类型枚举 */
export enum MenuItemTypeEnum {
  /** Web 应用 */
  Web = 'web',
  /** 小程序 */
  Miniprogram = 'miniprogram',
  /** 超链接 */
  Link = 'link',
  /** 变更日志 */
  Changelog = 'changelog'
}

/** 类型配置 */
export interface ItemTypeConfig {
  label: string
  shortLabel: string
  type: string
  icon: string
  emoji: string
  color: string
  bgColor: string
  badgeText: string
}

/** 类型标签信息 */
export interface TypeTagInfo {
  label: string
  type: string
  icon: string
  iconComponent: string
  color: string
  bgColor: string
  badgeText: string
}

/** 视口配置 */
export interface ViewportConfig {
  width: number
  height: number
  scalable?: boolean
  device?: string
}

/** 菜单项类型配置映射 */
export const ITEM_TYPES: Record<string, ItemTypeConfig> = {
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

/** 类型选项列表（用于下拉选择） */
export const TYPE_OPTIONS = [
  { value: 'web', label: 'Web 应用', icon: '🖥️' },
  { value: 'miniprogram', label: '小程序应用', icon: '📱' },
  { value: 'link', label: '超链接', icon: '🔗' },
  { value: 'changelog', label: '变更日志', icon: '📝' }
]

/** 默认视口配置 */
export const DEFAULT_VIEWPORT: ViewportConfig = { width: 375, height: 812 }

/** 给 URL 追加时间戳参数避免缓存 */
export function cacheBustUrl(url: string, key: number = Date.now()): string {
  if (!url) return ''
  const sep = url.includes('?') ? '&' : '?'
  return `${url}${sep}_t=${key}`
}

/** 获取类型标签信息 */
export function getTypeTag(type: string): TypeTagInfo {
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

/** 获取类型完整配置 */
export function getTypeConfig(type: string): ItemTypeConfig {
  return ITEM_TYPES[type] || ITEM_TYPES.web
}

/** 创建空的菜单项对象 */
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

/** 创建空的分组对象 */
export function createEmptyGroup(name: string = '新分组') {
  return {
    name,
    expanded: true,
    children: [] as unknown[]
  }
}

/** 重置表单数据到默认值 */
export function resetFormData(formData: Record<string, unknown>): void {
  const defaults = createEmptyMenuItem()
  for (const key of Object.keys(defaults)) {
    if (key === 'viewportConfig') {
      formData[key] = { ...DEFAULT_VIEWPORT }
    } else {
      formData[key] = defaults[key as keyof typeof defaults]
    }
  }
}
