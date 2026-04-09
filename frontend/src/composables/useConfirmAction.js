import { ElMessageBox, ElMessage } from 'element-plus'
import useAsyncLock from './useAsyncLock'

/**
 * 确认操作 composable（用于删除等需要确认的操作）
 * @param {object} options
 * @param {string} options.confirmMessage - 确认消息模板，{name} 会被替换为 item.name
 * @param {string} options.successMessage - 成功消息模板，{name} 会被替换
 * @param {Function} options.actionFn - 执行的异步操作函数，接收 (id) 参数
 * @param {Function} [options.onSuccess] - 成功后的回调
 * @returns {Function} confirmAndExecute(row) 函数
 */
export function useConfirmAction({ confirmMessage, successMessage, actionFn, onSuccess }) {
  const lock = useAsyncLock()

  return async function confirmAndExecute(row) {
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
}
