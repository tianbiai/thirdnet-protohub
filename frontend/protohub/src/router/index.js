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
        path: 'menu-editor',
        name: 'MenuEditor',
        component: () => import('@/views/menu-editor/index.vue'),
        meta: {
          title: '菜单编辑器',
          icon: 'Menu',
          permission: 'menu-editor',
          requireAdmin: true
        }
      },
      {
        path: 'projects',
        name: 'ProjectList',
        component: () => import('@/views/project/list.vue'),
        meta: {
          title: '项目列表',
          icon: 'Folder',
          permission: 'projects'
        }
      },
      {
        path: 'project/:id',
        name: 'ProjectDetail',
        component: () => import('@/views/project/detail.vue'),
        meta: {
          title: '项目详情',
          hidden: true,
          permission: 'project-detail'
        }
      },
      {
        path: 'content/:type/:id',
        name: 'ContentViewer',
        component: () => import('@/views/project/content.vue'),
        meta: {
          title: '内容查看',
          hidden: true,
          permission: 'content'
        }
      },
      {
        path: 'settings',
        name: 'Settings',
        component: () => import('@/views/settings/index.vue'),
        meta: {
          title: '系统设置',
          icon: 'Setting',
          permission: 'settings',
          requireAdmin: true
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
router.beforeEach((to, from, next) => {
  // 设置页面标题
  document.title = to.meta.title ? `${to.meta.title} - 项目管理基座` : '项目管理聚合基座'

  const userStore = useUserStore()
  const isLoggedIn = userStore.checkAuth()

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

  // 权限检查：需要管理员权限但当前用户不是管理员
  if (to.meta.requireAdmin && isLoggedIn && !userStore.isAdmin) {
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
