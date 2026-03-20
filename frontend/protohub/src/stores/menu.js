import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export const useMenuStore = defineStore('menu', () => {
  // 状态
  const menuConfig = ref({
    title: '项目管理聚合基座',
    version: '1.0.0',
    groups: []
  })
  const currentItemId = ref(null)
  const loading = ref(false)

  // 计算属性
  const groups = computed(() => menuConfig.value.groups || [])
  const currentItem = computed(() => {
    if (!currentItemId.value) return null
    for (const group of groups.value) {
      const item = group.children?.find(i => i.id === currentItemId.value)
      if (item) return item
    }
    return null
  })

  // 加载菜单配置（直接从静态文件读取，使用相对路径支持子目录部署）
  async function loadMenuConfig() {
    loading.value = true
    try {
      const response = await fetch('./menus.json')
      if (response.ok) {
        const data = await response.json()
        menuConfig.value = data
      }
    } catch (error) {
      console.error('加载菜单配置失败:', error)
    } finally {
      loading.value = false
    }
  }

  // 设置当前菜单项
  function setCurrentItem(itemId) {
    currentItemId.value = itemId
  }

  // 切换分组展开状态（仅内存状态，不持久化）
  function toggleGroupExpanded(groupId) {
    const group = menuConfig.value.groups.find(g => g.id === groupId)
    if (group) {
      group.expanded = !group.expanded
    }
  }

  return {
    menuConfig,
    currentItemId,
    currentItem,
    groups,
    loading,
    loadMenuConfig,
    setCurrentItem,
    toggleGroupExpanded
  }
})
