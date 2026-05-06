/**
 * 用户状态 Store
 * 管理认证、用户信息、角色权限
 */
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authApi } from '@/api/modules/app/auth'
import type { CurrentUserResponse } from '@/api/modules/app/auth'
import { getToken, setToken, getRefreshToken, setRefreshToken, clearToken } from '@/utils/token'

/** 映射后的用户信息结构 */
export interface MappedUserInfo {
  id: number
  userName: string
  nickName: string
  role: string
  roles: string[]
  roleNames: string[]
  permissions: string[]
  accessibleProjects: number[]
  manageableProjects: number[]
}

export const useUserStore = defineStore('user', () => {
  // ---- 状态 ----

  /** 访问令牌 */
  const token = ref(getToken())

  /** 刷新令牌 */
  const refreshTokenValue = ref(getRefreshToken())

  /** 用户信息 */
  const userInfo = ref<MappedUserInfo | null>((() => {
    try {
      return JSON.parse(localStorage.getItem('userInfo') || 'null')
    } catch {
      localStorage.removeItem('userInfo')
      return null
    }
  })())

  // ---- 计算属性 ----

  /** 是否已登录 */
  const isLoggedIn = computed(() => !!token.value && !!userInfo.value)

  /** 用户名 */
  const username = computed(() => userInfo.value?.userName || '')

  /** 昵称 */
  const nickname = computed(() => userInfo.value?.nickName || userInfo.value?.userName || '')

  /** 角色编码列表 */
  const roles = computed(() => userInfo.value?.roles || [])

  /** 角色名称列表 */
  const roleNames = computed(() => userInfo.value?.roleNames || [])

  /** 权限编码列表 */
  const permissions = computed(() => userInfo.value?.permissions || [])

  /** 是否为管理员 */
  const isAdmin = computed(() => roles.value.includes('admin'))

  // ---- 内部方法 ----

  /** 映射后端用户数据到前端结构 */
  function mapUserData(data: CurrentUserResponse): MappedUserInfo {
    return {
      id: data.id,
      userName: data.userName,
      nickName: data.nickName,
      role: data.roles?.[0]?.code || '',
      roles: (data.roles || []).map((r: { code: string }) => r.code),
      roleNames: (data.roles || []).map((r: { name: string }) => r.name),
      permissions: (data.permissions || []).map((p: { code: string }) => p.code),
      accessibleProjects: (data.projects || [])
        .filter((p: { accessType: string }) => p.accessType === 'view' || p.accessType === 'manage')
        .map((p: { id: number }) => p.id),
      manageableProjects: (data.projects || [])
        .filter((p: { accessType: string }) => p.accessType === 'manage')
        .map((p: { id: number }) => p.id)
    }
  }

  /** 保存用户信息到 state 和 localStorage */
  function saveUserInfo(mapped: MappedUserInfo) {
    userInfo.value = mapped
    localStorage.setItem('userInfo', JSON.stringify(mapped))
  }

  // ---- 操作方法 ----

  /** 登录 */
  async function login(loginUsername: string, loginPassword: string): Promise<MappedUserInfo> {
    // 第一步：获取 token
    const tokenResult = await authApi.login({
      username: loginUsername,
      password: loginPassword
    })

    token.value = tokenResult.access_token
    setToken(tokenResult.access_token)

    if (tokenResult.refresh_token) {
      refreshTokenValue.value = tokenResult.refresh_token
      setRefreshToken(tokenResult.refresh_token)
    }

    // 第二步：获取用户信息
    const data = await authApi.getCurrentUser()
    const mapped = mapUserData(data)
    saveUserInfo(mapped)
    return mapped
  }

  /** 登出 */
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

  /** 强制清除认证状态 */
  function forceLogout() {
    token.value = ''
    refreshTokenValue.value = ''
    userInfo.value = null
    clearToken()
    localStorage.removeItem('userInfo')
  }

  /** 刷新当前用户信息 */
  async function refreshUserInfo(): Promise<MappedUserInfo | null> {
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

  /** 修改密码 */
  async function changePassword(oldPassword: string, newPassword: string) {
    return await authApi.changePassword({ oldPassword, newPassword })
  }

  /** 检查登录状态 */
  function checkAuth(): boolean {
    return !!token.value && !!userInfo.value
  }

  /** 检查是否有指定权限（支持 view 匹配 view-all） */
  function hasPermission(permission: string): boolean {
    if (permissions.value.includes(permission)) return true
    if (permission.endsWith(':view')) {
      return permissions.value.includes(permission + '-all')
    }
    return false
  }

  /** 检查是否拥有某个模块的任意操作权限 */
  function hasModulePermission(module: string): boolean {
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
