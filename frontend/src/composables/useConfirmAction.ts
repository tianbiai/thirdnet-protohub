/**
 * 确认操作 composable
 * 用于删除等需要确认的操作
 */
import { ElMessageBox, ElMessage } from 'element-plus'
import { useAsyncLock } from '@/composables/useAsyncLock'

interface UseConfirmActionOptions<T> {
  /** 确认消息（支持函数动态生成） */
  confirmMessage: string | ((row: T) => string)
  /** 成功消息（支持函数动态生成） */
  successMessage: string | ((row: T) => string)
  /** 执行的异步操作 */
  actionFn: (id: number | string) => Promise<unknown>
  /** 成功后的回调 */
  onSuccess?: () => void
}

export function useConfirmAction<T extends { id: number | string }>({
  confirmMessage,
  successMessage,
  actionFn,
  onSuccess
}: UseConfirmActionOptions<T>) {
  const lock = useAsyncLock()

  /** 确认并执行操作 */
  async function confirmAndExecute(row: T) {
    await lock.forKey(row.id, async () => {
      await ElMessageBox.confirm(
        typeof confirmMessage === 'function' ? confirmMessage(row) : confirmMessage,
        '确认操作',
        { confirmButtonText: '确定', cancelButtonText: '取消', type: 'warning' }
      )

      await actionFn(row.id)

      ElMessage.success(
        typeof successMessage === 'function' ? successMessage(row) : successMessage
      )

      onSuccess?.()
    })()
  }

  return { confirmAndExecute }
}
