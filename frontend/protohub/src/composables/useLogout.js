import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { useUserStore } from '@/stores/user'

/**
 * 登出功能 composable
 * 提供统一的登出逻辑， */
export function useLogout() {
  const router = useRouter()
  const userStore = useUserStore()

  /**
   * 处理登出操作
   * 包含确认对话框和登出后的跳转
   */
  async function handleLogout() {
    try {
      await ElMessageBox.confirm('确定要退出登录吗？', '提示', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      })

      userStore.logout()
      ElMessage.success('已退出登录')
      router.push('/login')
    } catch {
      // 用户取消操作，无需处理
    }
  }

  return {
    handleLogout
  }
}

export default useLogout
