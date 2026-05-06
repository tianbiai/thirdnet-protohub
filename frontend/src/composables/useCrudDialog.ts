/**
 * CRUD 对话框通用逻辑
 */
import { ref, reactive } from 'vue'
import { ElMessage } from 'element-plus'
import type { FormInstance } from 'element-plus'

interface UseCrudDialogOptions<T> {
  /** 表单默认值 */
  defaults: T
  /** 创建 API 函数 */
  createFn: (data: T) => Promise<unknown>
  /** 更新 API 函数 */
  updateFn: (data: T) => Promise<unknown>
  /** 实体名称（用于提示消息） */
  entityName: string
  /** 成功后的回调 */
  onSuccess?: () => void
}

/** 深拷贝辅助函数 */
function deepClone<T>(obj: T): T {
  return JSON.parse(JSON.stringify(obj))
}

export function useCrudDialog<T extends Record<string, unknown>>({
  defaults,
  createFn,
  updateFn,
  entityName,
  onSuccess
}: UseCrudDialogOptions<T>) {
  /** 对话框是否可见 */
  const dialogVisible = ref(false)

  /** 是否为编辑模式 */
  const isEdit = ref(false)

  /** 提交按钮加载状态 */
  const submitLoading = ref(false)

  /** 表单引用 */
  const formRef = ref<FormInstance | null>(null)

  /** 表单数据 */
  const formData = reactive<Record<string, unknown>>({ ...deepClone(defaults) })

  /** 显示创建对话框 */
  function showCreate() {
    isEdit.value = false
    resetForm()
    dialogVisible.value = true
  }

  /** 显示编辑对话框 */
  function showEdit(row: Record<string, unknown>) {
    isEdit.value = true
    Object.assign(formData, { ...deepClone(defaults), ...deepClone(row) })
    dialogVisible.value = true
  }

  /** 重置表单 */
  function resetForm() {
    formRef.value?.resetFields()
    Object.assign(formData, { ...deepClone(defaults) })
  }

  /** 提交表单 */
  async function handleSubmit() {
    try {
      await formRef.value?.validate()
    } catch {
      return
    }

    submitLoading.value = true
    try {
      if (isEdit.value) {
        await updateFn(formData as T)
        ElMessage.success(`${entityName}更新成功`)
      } else {
        await createFn(formData as T)
        ElMessage.success(`${entityName}创建成功`)
      }
      dialogVisible.value = false
      onSuccess?.()
    } catch (error) {
      const message = error instanceof Error ? error.message : '操作失败'
      ElMessage.error(message)
    } finally {
      submitLoading.value = false
    }
  }

  return {
    dialogVisible,
    isEdit,
    submitLoading,
    formRef,
    formData,
    showCreate,
    showEdit,
    resetForm,
    handleSubmit
  }
}
