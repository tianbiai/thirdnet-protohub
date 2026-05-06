/**
 * 应用全局状态 Store
 * 管理 UI 状态：侧边栏、主题、面包屑等
 */
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export const useAppStore = defineStore('app', () => {
  // ---- 状态 ----

  /** 侧边栏是否折叠 */
  const sidebarCollapsed = ref(localStorage.getItem('sidebarCollapsed') === 'true')

  /** 当前主题（light/dark） */
  const theme = ref(localStorage.getItem('theme') || 'light')

  /** 内容区加载状态 */
  const contentLoading = ref(false)

  /** 面包屑导航 */
  const breadcrumb = ref<Array<{ title: string; path?: string }>>([])

  // ---- 计算属性 ----

  /** 是否为暗色主题 */
  const isDarkTheme = computed(() => theme.value === 'dark')

  // ---- 操作方法 ----

  /** 切换侧边栏折叠状态 */
  function toggleSidebar() {
    sidebarCollapsed.value = !sidebarCollapsed.value
    localStorage.setItem('sidebarCollapsed', String(sidebarCollapsed.value))
  }

  /** 设置侧边栏状态 */
  function setSidebarCollapsed(collapsed: boolean) {
    sidebarCollapsed.value = collapsed
    localStorage.setItem('sidebarCollapsed', String(collapsed))
  }

  /** 同步主题到 DOM（data-theme 属性 + is-dark 类名） */
  function applyThemeToDOM(t: string) {
    document.documentElement.setAttribute('data-theme', t)
    document.documentElement.classList.toggle('is-dark', t === 'dark')
  }

  /** 切换主题 */
  function toggleTheme() {
    theme.value = theme.value === 'light' ? 'dark' : 'light'
    localStorage.setItem('theme', theme.value)
    applyThemeToDOM(theme.value)
  }

  /** 设置主题 */
  function setTheme(newTheme: string) {
    theme.value = newTheme
    localStorage.setItem('theme', newTheme)
    applyThemeToDOM(newTheme)
  }

  /** 设置内容加载状态 */
  function setContentLoading(loading: boolean) {
    contentLoading.value = loading
  }

  /** 设置面包屑 */
  function setBreadcrumb(items: Array<{ title: string; path?: string }>) {
    breadcrumb.value = items
  }

  /** 初始化主题（应用启动时调用） */
  function initTheme() {
    applyThemeToDOM(theme.value)
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
