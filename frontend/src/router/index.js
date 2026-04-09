import { createRouter, createWebHashHistory } from 'vue-router'
import { useUserStore } from '@/stores/user'

// 路由配置
const routes = [
  {
    path: '/login',
    name: 'Login',
    component: () => import('@/views/login/index.vue'),
    meta: {
      title: '登录',
      requiresAuth: false
    }
  },
  {
    path: '/',
    component: () => import('@/components/Layout/index.vue'),
    meta: { requiresAuth: true },
    children: [
      {
        path: '',
        name: 'Home',
        component: () => import('@/views/home/index.vue'),
        meta: {
          title: '首页'
        }
      },
      {
        path: 'projects',
        name: 'ProjectList',
        component: () => import('@/views/project/list.vue'),
        meta: {
          title: '项目管理',
          icon: 'Folder',
          permission: 'projects:view'
        }
      },
      {
        path: 'project/:id',
        name: 'ProjectDetail',
        component: () => import('@/views/project/detail.vue'),
        meta: {
          title: '项目详情',
          hidden: true
        }
      },
      {
        path: 'content/:type/:id',
        name: 'ContentViewer',
        component: () => import('@/views/project/content.vue'),
        meta: {
          title: '内容查看',
          hidden: true
        }
      },
      {
        path: 'settings',
        name: 'Settings',
        component: () => import('@/views/settings/index.vue'),
        meta: {
          title: '系统设置',
          icon: 'Setting',
          permission: 'settings'
        }
      },
      // 系统管理
      {
        path: 'system/users',
        name: 'UserManage',
        component: () => import('@/views/system/user/index.vue'),
        meta: {
          title: '用户管理',
          icon: 'User',
          permission: 'user-manage:view'
        }
      },
      {
        path: 'system/roles',
        name: 'RoleManage',
        component: () => import('@/views/system/role/index.vue'),
        meta: {
          title: '角色管理',
          icon: 'UserFilled',
          permission: 'role-manage:view'
        }
      },
      {
        path: 'system/permissions',
        name: 'PermissionManage',
        component: () => import('@/views/system/permission/index.vue'),
        meta: {
          title: '权限管理',
          icon: 'Lock',
          permission: 'permission-manage:view'
        }
      },
      // 项目管理
      {
        path: 'project-access',
        name: 'ProjectAccess',
        component: () => import('@/views/project/access.vue'),
        meta: {
          title: '项目成员',
          icon: 'Avatar',
          permission: 'project-access:view'
        }
      },
      // 系统功能菜单
      {
        path: 'system/menus',
        name: 'SystemMenuManage',
        component: () => import('@/views/system/menu/index.vue'),
        meta: {
          title: '系统菜单',
          icon: 'Menu',
          permission: 'system-menu-manage:view'
        }
      }
    ]
  },
  {
    path: '/:pathMatch(.*)*',
    name: 'NotFound',
    component: () => import('@/views/error/404.vue'),
    meta: {
      title: '页面未找到',
      requiresAuth: false
    }
  }
]

// 创建路由实例
const router = createRouter({
  history: createWebHashHistory(),
  routes
})

// 路由守卫
router.beforeEach(async (to, from, next) => {
  // 设置页面标题
  document.title = to.meta.title ? `${to.meta.title} - 项目管理基座` : '项目管理聚合基座'

  const userStore = useUserStore()
  const isLoggedIn = await userStore.checkAuth()

  // 需要认证但未登录
  if (to.meta.requiresAuth !== false && !isLoggedIn) {
    next({
      name: 'Login',
      query: { redirect: to.fullPath }
    })
    return
  }

  // 已登录但访问登录页
  if (to.name === 'Login' && isLoggedIn) {
    next({ name: 'Home' })
    return
  }

  // 权限检查：检查用户是否有访问该页面的权限
  if (to.meta.permission && isLoggedIn && !userStore.hasPermission(to.meta.permission)) {
    next({ name: 'Home' })
    return
  }

  next()
})

export default router
