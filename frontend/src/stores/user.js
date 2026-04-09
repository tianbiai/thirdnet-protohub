import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import * as authApi from '@/api/auth'

export const useUserStore = defineStore('user', () => {
  // 状态
  const token = ref(localStorage.getItem('token') || '')
  const refreshTokenValue = ref(localStorage.getItem('refreshToken') || '')
  const userInfo = ref((() => {
    try {
      return JSON.parse(localStorage.getItem('userInfo') || 'null')
    } catch {
      localStorage.removeItem('userInfo')
      return null
    }
  })())

  // 计算属性
  const isLoggedIn = computed(() => !!token.value && !!userInfo.value)
  const username = computed(() => userInfo.value?.username || '')
  const nickname = computed(() => userInfo.value?.nickname || userInfo.value?.username || '')
  const roles = computed(() => userInfo.value?.roles || [])
  const roleNames = computed(() => userInfo.value?.roleNames || [])
  const permissions = computed(() => userInfo.value?.permissions || [])

  // 是否为管理员
  const isAdmin = computed(() => {
    return roles.value.includes('admin')
  })

  /**
   * 检查登录状态
   * 使用 HMAC-SHA512 签名的 Basic 认证，无需 JWT token 刷新
   * - 有 token 和用户信息 → 已登录
   * - 无 token 或用户信息 → 未登录
   */
  function checkAuth() {
    return !!token.value && !!userInfo.value
  }

  /**
   * 强制清除认证状态并跳转登录页
   */
  function forceLogout() {
    token.value = ''
    refreshTokenValue.value = ''
    userInfo.value = null
    localStorage.removeItem('token')
    localStorage.removeItem('refreshToken')
    localStorage.removeItem('userInfo')
  }

  // 映射后端用户数据到前端结构
  function mapUserData(data) {
    return {
      id: data.id,
      username: data.userName,
      nickname: data.nickName,
      role: data.roles?.[0]?.code || '',
      roles: (data.roles || []).map(r => r.code),
      roleNames: (data.roles || []).map(r => r.name),
      permissions: (data.permissions || []).map(p => p.code),
      accessibleProjects: (data.projects || [])
        .filter(p => p.accessType === 'view' || p.accessType === 'manage')
        .map(p => p.id),
      manageableProjects: (data.projects || [])
        .filter(p => p.accessType === 'manage')
        .map(p => p.id)
    }
  }

  // 保存用户信息到 state 和 localStorage
  function saveUserInfo(mapped) {
    userInfo.value = mapped
    localStorage.setItem('userInfo', JSON.stringify(mapped))
  }

  // 登录
  async function login(username, password) {
    // 第一步：获取 token（scope 含 offline_access，后端会返回 refresh_token）
    const tokenResult = await authApi.getToken(username, password)

    token.value = tokenResult.access_token
    localStorage.setItem('token', tokenResult.access_token)

    if (tokenResult.refresh_token) {
      refreshTokenValue.value = tokenResult.refresh_token
      localStorage.setItem('refreshToken', tokenResult.refresh_token)
    }

    // 第二步：获取用户信息（经 transform 后已是 camelCase）
    const data = await authApi.getCurrentUser()
    const mapped = mapUserData(data)
    saveUserInfo(mapped)
    return mapped
  }

  // 登出
  async function logout() {
    try {
      if (token.value) {
        await authApi.logout()
      }
    } catch (error) {
      console.warn('登出请求失败:', error)
    } finally {
      forceLogout()
    }
  }

  // 获取当前用户信息（刷新）
  async function refreshUserInfo() {
    if (!token.value) return null
    try {
      const data = await authApi.getCurrentUser()
      const mapped = mapUserData(data)
      saveUserInfo(mapped)
      return mapped
    } catch (error) {
      console.warn('刷新用户信息失败:', error)
      return userInfo.value
    }
  }

  // 修改密码
  async function changePassword(oldpassword, newpassword) {
    return await authApi.changePassword(oldpassword, newpassword)
  }

  // 检查是否有指定的权限
  // 支持 view 匹配 view-all：如用户持有 projects:view-all，hasPermission('projects:view') 也返回 true
  function hasPermission(permission) {
    if (permissions.value.includes(permission)) return true
    if (permission.endsWith(':view')) {
      return permissions.value.includes(permission + '-all')
    }
    return false
  }

  // 检查是否拥有某个模块的任意操作权限
  function hasModulePermission(module) {
    return permissions.value.some(p => p.startsWith(module + ':'))
  }

  return {
    token,
    userInfo,
    isLoggedIn,
    username,
    nickname,
    roles,
    roleNames,
    permissions,
    isAdmin,
    login,
    logout,
    refreshUserInfo,
    changePassword,
    checkAuth,
    hasPermission,
    hasModulePermission,
    forceLogout
  }
})
