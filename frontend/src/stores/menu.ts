/**
 * 菜单状态 Store
 * 管理应用端菜单配置和管理端菜单 CRUD
 */
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { menuApi } from '@/api/modules/app/menu'
import { managerMenuApi } from '@/api/modules/manager/menu'
import type { MenuGroup, MenuItem, MenuConfigResponse } from '@/api/modules/app/menu'

/** 内部菜单分组结构（含前端扩展字段） */
export interface InternalMenuGroup extends Omit<MenuGroup, 'items'> {
  children: MenuItem[]
  expanded: boolean
}

/** 内部菜单配置结构 */
export interface InternalMenuConfig {
  title: string
  version: string
  groups: InternalMenuGroup[]
}

export const useMenuStore = defineStore('menu', () => {
  // ---- 状态 ----

  /** 菜单配置 */
  const menuConfig = ref<InternalMenuConfig>({
    title: '项目管理聚合基座',
    version: '1.0.0',
    groups: []
  })

  /** 当前选中的菜单项 ID */
  const currentItemId = ref<number | null>(null)

  /** 加载状态 */
  const loading = ref(false)

  /** 是否已加载 */
  const loaded = ref(false)

  // ---- 计算属性 ----

  /** 分组列表 */
  const groups = computed(() => menuConfig.value.groups || [])

  /** 标题 */
  const title = computed(() => menuConfig.value.title || '项目管理聚合基座')

  /** 当前选中的菜单项 */
  const currentItem = computed(() => {
    if (!currentItemId.value) return null
    for (const group of groups.value) {
      const item = group.children?.find(
        (i: MenuItem) => String(i.id) === String(currentItemId.value)
      )
      if (item) return item
    }
    return null
  })

  // ---- 应用端方法 ----

  /** 加载菜单配置 */
  async function loadMenuConfig(forceRefresh = false): Promise<InternalMenuConfig> {
    if (loaded.value && !forceRefresh) return menuConfig.value

    loading.value = true
    try {
      const response = await menuApi.getMenuConfig()
      const groupList = Array.isArray(response) ? response : (response.groups || [])
      menuConfig.value = {
        title: (response as MenuConfigResponse).title || '项目管理聚合基座',
        version: (response as MenuConfigResponse).version || '1.0.0',
        groups: groupList.map((g: MenuGroup) => ({
          ...g,
          children: g.items || g.children || [],
          expanded: true
        }))
      }
      loaded.value = true
    } catch (error) {
      console.error('加载菜单配置失败:', error)
      throw error
    } finally {
      loading.value = false
    }

    return menuConfig.value
  }

  /** 重置菜单状态（登出时调用） */
  function resetMenu() {
    menuConfig.value = {
      title: '项目管理聚合基座',
      version: '1.0.0',
      groups: []
    }
    currentItemId.value = null
    loaded.value = false
  }

  /** 设置当前菜单项 */
  function setCurrentItem(itemId: number | null) {
    currentItemId.value = itemId
  }

  /** 根据 ID 查找菜单项 */
  function findItemById(itemId: number | null): MenuItem | null {
    if (!itemId) return null
    for (const group of groups.value) {
      const item = group.children?.find(
        (i: MenuItem) => i.id === itemId
      )
      if (item) return item
    }
    return null
  }

  /** 切换分组展开状态 */
  function toggleGroupExpanded(groupId: number) {
    const group = menuConfig.value.groups.find(g => g.id === groupId)
    if (group) {
      group.expanded = !group.expanded
      persistExpandedState()
    }
  }

  /** 持久化展开状态到 localStorage */
  function persistExpandedState() {
    const expandedIds = menuConfig.value.groups
      .filter(g => g.expanded)
      .map(g => g.id)
    localStorage.setItem('expandedGroups', JSON.stringify(expandedIds))
  }

  /** 从 localStorage 恢复展开状态 */
  function restoreExpandedState() {
    try {
      const saved = JSON.parse(localStorage.getItem('expandedGroups') || 'null')
      if (Array.isArray(saved)) {
        menuConfig.value.groups.forEach(group => {
          group.expanded = saved.includes(group.id)
        })
      }
    } catch {
      localStorage.removeItem('expandedGroups')
    }
  }

  // ---- 管理端方法 ----

  /** 获取分组列表 */
  async function getGroupList() {
    return await managerMenuApi.getGroupList()
  }

  /** 创建分组 */
  async function createGroup(data: { name: string; icon?: string }) {
    const result = await managerMenuApi.createGroup(data)
    await loadMenuConfig(true)
    return result
  }

  /** 更新分组 */
  async function updateGroup(data: { id: number; name?: string; icon?: string }) {
    const result = await managerMenuApi.updateGroup(data)
    await loadMenuConfig(true)
    return result
  }

  /** 删除分组 */
  async function deleteGroup(id: number) {
    await managerMenuApi.deleteGroup(id)
    await loadMenuConfig(true)
  }

  /** 分组排序 */
  async function reorderGroups(orders: Array<{ id: number }>) {
    await managerMenuApi.reorderGroups(orders.map(o => o.id))
    await loadMenuConfig(true)
  }

  /** 获取菜单项列表 */
  async function getItemList(groupId: number) {
    return await managerMenuApi.getItemList(groupId)
  }

  /** 创建菜单项 */
  async function createItem(data: Record<string, unknown>) {
    const result = await managerMenuApi.createItem(data)
    await loadMenuConfig(true)
    return result
  }

  /** 更新菜单项 */
  async function updateItem(data: Record<string, unknown>) {
    const result = await managerMenuApi.updateItem(data)
    await loadMenuConfig(true)
    return result
  }

  /** 删除菜单项 */
  async function deleteItem(id: number) {
    await managerMenuApi.deleteItem(id)
    await loadMenuConfig(true)
  }

  /** 菜单项排序 */
  async function reorderItems(groupId: number, orders: Array<{ id: number }>) {
    await managerMenuApi.reorderItems(groupId, orders.map(o => o.id))
    await loadMenuConfig(true)
  }

  return {
    menuConfig,
    currentItemId,
    currentItem,
    groups,
    title,
    loading,
    loaded,
    loadMenuConfig,
    resetMenu,
    setCurrentItem,
    findItemById,
    toggleGroupExpanded,
    restoreExpandedState,
    getGroupList,
    createGroup,
    updateGroup,
    deleteGroup,
    reorderGroups,
    getItemList,
    createItem,
    updateItem,
    deleteItem,
    reorderItems
  }
})
