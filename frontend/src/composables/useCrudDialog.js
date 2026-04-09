import { ref, reactive } from 'vue'
import { ElMessage } from 'element-plus'

/**
 * CRUD 对话框通用逻辑
 * @param {object} options
 * @param {object} options.defaults - 表单默认值对象
 * @param {Function} options.createFn - 创建 API 函数
 * @param {Function} options.updateFn - 更新 API 函数
 * @param {string} options.entityName - 实体名称（用于提示消息）
 * @param {Function} [options.onSuccess] - 成功后的回调
 * @returns {object}
 */
export function useCrudDialog({ defaults, createFn, updateFn, entityName, onSuccess }) {
  const dialogVisible = ref(false)
  const isEdit = ref(false)
  const submitLoading = ref(false)
  const formRef = ref(null)

  const formData = reactive({ ...defaults })

  function showCreate() {
    isEdit.value = false
    resetForm()
    dialogVisible.value = true
  }

  function showEdit(row) {
    isEdit.value = true
    Object.assign(formData, { ...defaults, ...row })
    dialogVisible.value = true
  }

  function resetForm() {
    formRef.value?.resetFields()
    Object.assign(formData, { ...defaults })
  }

  async function handleSubmit() {
    try {
      await formRef.value?.validate()
    } catch {
      return
    }

    submitLoading.value = true
    try {
      if (isEdit.value) {
        await updateFn(formData)
        ElMessage.success(`${entityName}更新成功`)
      } else {
        await createFn(formData)
        ElMessage.success(`${entityName}创建成功`)
      }
      dialogVisible.value = false
      onSuccess?.()
    } catch (error) {
      ElMessage.error(error.message || '操作失败')
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
