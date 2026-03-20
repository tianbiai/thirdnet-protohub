import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

const USER_CONFIG_KEY = 'manager-user-config'

// 默认权限配置（作为后备）
const defaultPermissions = {
  admin: ['menu-editor', 'projects', 'project-detail', 'content', 'settings'],
  guest: ['projects', 'project-detail', 'content']
}

export const useUserStore = defineStore('user', () => {
  // 状态
  const token = ref(localStorage.getItem('token') || '')
  const userInfo = ref(JSON.parse(localStorage.getItem('userInfo') || 'null'))
  const userConfig = ref(null)
  const configLoaded = ref(false)

  // 计算属性
  const isLoggedIn = computed(() => !!token.value)
  const username = computed(() => userInfo.value?.username || '')
  const nickname = computed(() => userInfo.value?.nickname || userInfo.value?.username || '')
  const role = computed(() => userInfo.value?.role || 'guest')

  // 是否为管理员
  const isAdmin = computed(() => {
    return role.value === 'admin'
  })

  // 是否可以编辑菜单
  const canEditMenu = computed(() => {
    if (!userConfig.value?.permissions?.[role.value]) {
      return role.value === 'admin'
    }
    return userConfig.value.permissions[role.value].canEditMenu === true
  })

  // 加载用户配置
  async function loadUserConfig() {
    if (configLoaded.value) return userConfig.value

    try {
      // 优先从 localStorage 读取缓存
      const cached = localStorage.getItem(USER_CONFIG_KEY)
      if (cached) {
        userConfig.value = JSON.parse(cached)
        configLoaded.value = true
        return userConfig.value
      }

      // 从静态文件加载（使用相对路径，支持子目录部署）
      const response = await fetch('./users.json')
      if (response.ok) {
        const data = await response.json()
        userConfig.value = data
        localStorage.setItem(USER_CONFIG_KEY, JSON.stringify(data))
        configLoaded.value = true
        return data
      }
    } catch (error) {
      console.error('加载用户配置失败:', error)
    }

    return null
  }

  // 登录
  async function login(username, password) {
    // 确保配置已加载
    const config = await loadUserConfig()

    // 从配置中获取用户列表
    const validUsers = config?.users || []

    const user = validUsers.find(u => u.username === username && u.password === password)

    if (user) {
      token.value = 'token-' + Date.now() + '-' + Math.random().toString(36).slice(2)
      userInfo.value = {
        id: user.id,
        username: user.username,
        nickname: user.nickname,
        role: user.role
      }
      localStorage.setItem('token', token.value)
      localStorage.setItem('userInfo', JSON.stringify(userInfo.value))
      return userInfo.value
    } else {
      throw new Error('用户名或密码错误')
    }
  }

  // 登出
  function logout() {
    token.value = ''
    userInfo.value = null
    localStorage.removeItem('token')
    localStorage.removeItem('userInfo')
  }

  // 检查登录状态
  function checkAuth() {
    return !!token.value && !!userInfo.value
  }

  // 检查是否有访问指定页面的权限
  function hasPermission(pageName) {
    const userRole = role.value || 'guest'

    // 优先使用配置文件中的权限
    if (userConfig.value?.permissions?.[userRole]?.features) {
      return userConfig.value.permissions[userRole].features.includes(pageName)
    }

    // 后备使用默认权限
    const allowedPages = defaultPermissions[userRole] || defaultPermissions.guest
    return allowedPages.includes(pageName)
  }

  // 获取当前角色的权限信息
  function getRoleInfo() {
    const userRole = role.value || 'guest'
    if (userConfig.value?.permissions?.[userRole]) {
      return userConfig.value.permissions[userRole]
    }
    return {
      label: userRole === 'admin' ? '管理员' : '访客',
      features: defaultPermissions[userRole] || defaultPermissions.guest,
      canEditMenu: userRole === 'admin'
    }
  }

  return {
    token,
    userInfo,
    userConfig,
    isLoggedIn,
    username,
    nickname,
    role,
    isAdmin,
    canEditMenu,
    loadUserConfig,
    login,
    logout,
    checkAuth,
    hasPermission,
    getRoleInfo
  }
})
