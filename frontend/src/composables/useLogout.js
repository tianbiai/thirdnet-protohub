import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { useUserStore } from '@/stores/user'
import { useMenuStore } from '@/stores/menu'
import { useAsyncLock } from '@/composables/useAsyncLock'

/**
 * 登出功能 composable
 * 提供统一的登出逻辑
 */
export function useLogout() {
  const router = useRouter()
  const userStore = useUserStore()
  const menuStore = useMenuStore()
  const logoutLock = useAsyncLock()

  /**
   * 处理登出操作
   * 包含确认对话框和登出后的跳转
   */
  const handleLogout = logoutLock.wrap(async () => {
    await ElMessageBox.confirm('确定要退出登录吗？', '提示', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning'
    })

    try {
      await userStore.logout()
      ElMessage.success('已退出登录')
    } catch {
      ElMessage.warning('登录已过期，请重新登录')
    }

    try {
      menuStore.resetMenu()
    } catch {
      // resetMenu 失败不影响跳转
    }

    // 统一在此处跳转，确保始终执行
    if (router.currentRoute.value.name !== 'Login') {
      await router.push({ name: 'Login' })
    }
  })

  return {
    handleLogout
  }
}

export default useLogout
