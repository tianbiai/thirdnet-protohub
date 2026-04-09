import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export const useAppStore = defineStore('app', () => {
  // 状态
  const sidebarCollapsed = ref(localStorage.getItem('sidebarCollapsed') === 'true')
  const theme = ref(localStorage.getItem('theme') || 'light')
  const contentLoading = ref(false)
  const breadcrumb = ref([])

  // 计算属性
  const isDarkTheme = computed(() => theme.value === 'dark')

  // 切换侧边栏
  function toggleSidebar() {
    sidebarCollapsed.value = !sidebarCollapsed.value
    localStorage.setItem('sidebarCollapsed', sidebarCollapsed.value)
  }

  // 设置侧边栏状态
  function setSidebarCollapsed(collapsed) {
    sidebarCollapsed.value = collapsed
    localStorage.setItem('sidebarCollapsed', collapsed)
  }

  // 切换主题
  function toggleTheme() {
    theme.value = theme.value === 'light' ? 'dark' : 'light'
    localStorage.setItem('theme', theme.value)
    document.documentElement.setAttribute('data-theme', theme.value)
  }

  // 设置主题
  function setTheme(newTheme) {
    theme.value = newTheme
    localStorage.setItem('theme', newTheme)
    document.documentElement.setAttribute('data-theme', newTheme)
  }

  // 设置内容加载状态
  function setContentLoading(loading) {
    contentLoading.value = loading
  }

  // 设置面包屑
  function setBreadcrumb(items) {
    breadcrumb.value = items
  }

  // 初始化主题
  function initTheme() {
    document.documentElement.setAttribute('data-theme', theme.value)
  }

  return {
    sidebarCollapsed,
    theme,
    contentLoading,
    breadcrumb,
    isDarkTheme,
    toggleSidebar,
    setSidebarCollapsed,
    toggleTheme,
    setTheme,
    setContentLoading,
    setBreadcrumb,
    initTheme
  }
})
