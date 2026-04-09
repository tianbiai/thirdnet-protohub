import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import * as menuApi from '@/api/menu'

export const useMenuStore = defineStore('menu', () => {
  // 状态
  const menuConfig = ref({
    title: '项目管理聚合基座',
    version: '1.0.0',
    groups: []
  })
  const currentItemId = ref(null)
  const loading = ref(false)
  const loaded = ref(false)

  // 计算属性
  const groups = computed(() => menuConfig.value.groups || [])
  const title = computed(() => menuConfig.value.title || '项目管理聚合基座')
  const currentItem = computed(() => {
    if (!currentItemId.value) return null
    for (const group of groups.value) {
      const item = group.children?.find(i => String(i.id) === String(currentItemId.value))
      if (item) return item
    }
    return null
  })

  // 加载菜单配置
  async function loadMenuConfig(forceRefresh = false) {
    if (loaded.value && !forceRefresh) return menuConfig.value

    loading.value = true
    try {
      const response = await menuApi.getMenuConfig()
      // 后端返回的是分组数组（不是 { title, groups } 包装对象）
      const groups = Array.isArray(response) ? response : (response.groups || [])
      menuConfig.value = {
        title: response.title || '项目管理聚合基座',
        version: response.version || '1.0.0',
        groups: groups.map(g => ({
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

  // 重置菜单状态（登出时调用）
  function resetMenu() {
    menuConfig.value = {
      title: '项目管理聚合基座',
      version: '1.0.0',
      groups: []
    }
    currentItemId.value = null
    loaded.value = false
  }

  // 设置当前菜单项
  function setCurrentItem(itemId) {
    currentItemId.value = itemId
  }

  // 根据 ID 查找菜单项
  function findItemById(itemId) {
    if (!itemId) return null
    for (const group of groups.value) {
      const item = group.children?.find(i => String(i.id) === String(itemId))
      if (item) return item
    }
    return null
  }

  // 切换分组展开状态
  function toggleGroupExpanded(groupId) {
    const group = menuConfig.value.groups.find(g => g.id === groupId)
    if (group) {
      group.expanded = !group.expanded
      persistExpandedState()
    }
  }

  // 持久化展开状态到 localStorage
  function persistExpandedState() {
    const expandedIds = menuConfig.value.groups
      .filter(g => g.expanded)
      .map(g => g.id)
    localStorage.setItem('expandedGroups', JSON.stringify(expandedIds))
  }

  // 从 localStorage 恢复展开状态
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

  // ==================== 管理端操作 ====================

  // 获取分组列表（管理端）
  async function getGroupList() {
    return await menuApi.getGroupList()
  }

  // 创建分组
  async function createGroup(data) {
    const result = await menuApi.createGroup(data)
    // 刷新菜单配置
    await loadMenuConfig(true)
    return result
  }

  // 更新分组
  async function updateGroup(data) {
    const result = await menuApi.updateGroup(data)
    // 刷新菜单配置
    await loadMenuConfig(true)
    return result
  }

  // 删除分组
  async function deleteGroup(id) {
    await menuApi.deleteGroup(id)
    // 刷新菜单配置
    await loadMenuConfig(true)
  }

  // 分组排序
  async function reorderGroups(orders) {
    await menuApi.reorderGroups(orders)
    // 刷新菜单配置
    await loadMenuConfig(true)
  }

  // 获取菜单项列表（管理端）
  async function getItemList(groupid) {
    return await menuApi.getItemList(groupid)
  }

  // 创建菜单项
  async function createItem(data) {
    const result = await menuApi.createItem(data)
    // 刷新菜单配置
    await loadMenuConfig(true)
    return result
  }

  // 更新菜单项
  async function updateItem(data) {
    const result = await menuApi.updateItem(data)
    // 刷新菜单配置
    await loadMenuConfig(true)
    return result
  }

  // 删除菜单项
  async function deleteItem(id) {
    await menuApi.deleteItem(id)
    // 刷新菜单配置
    await loadMenuConfig(true)
  }

  // 菜单项排序
  async function reorderItems(groupid, orders) {
    await menuApi.reorderItems(groupid, orders)
    // 刷新菜单配置
    await loadMenuConfig(true)
  }

  return {
    // 状态
    menuConfig,
    currentItemId,
    currentItem,
    groups,
    title,
    loading,
    loaded,
    // 应用端方法
    loadMenuConfig,
    resetMenu,
    setCurrentItem,
    findItemById,
    toggleGroupExpanded,
    restoreExpandedState,
    // 管理端方法
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
