import { ref } from 'vue'
import { ElMessage } from 'element-plus'

const FILES_STORAGE_KEY = 'manager-uploaded-files'

/**
 * 生成唯一 ID
 */
function generateId(prefix = 'doc') {
  return `${prefix}-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`
}

/**
 * 获取已上传的文件列表
 */
function getStoredFiles() {
  try {
    const data = localStorage.getItem(FILES_STORAGE_KEY)
    return data ? JSON.parse(data) : {}
  } catch {
    return {}
  }
}

/**
 * 保存文件列表
 */
function setStoredFiles(files) {
  localStorage.setItem(FILES_STORAGE_KEY, JSON.stringify(files))
}

/**
 * 文件上传相关逻辑的 composable
 */
export function useFileUpload() {
  const pendingFile = ref(null)
  const uploadLoading = ref(false)

  /**
   * 处理文件选择
   * @param {Object} uploadFile - el-upload 传入的文件对象
   * @param {Object} options - 配置选项
   * @param {string[]} options.allowedTypes - 允许的文件类型
   * @returns {boolean} 是否选择成功
   */
  function handleFileSelect(uploadFile, options = {}) {
    const {
      allowedTypes = ['.md', '.markdown']
    } = options

    const file = uploadFile.raw
    if (!file) return false

    // 验证文件类型
    const fileName = file.name.toLowerCase()
    const isAllowed = allowedTypes.some(ext => fileName.endsWith(ext))

    if (!isAllowed) {
      ElMessage.error(`请选择 ${allowedTypes.join(' 或 ')} 文件`)
      return false
    }

    pendingFile.value = file
    return true
  }

  /**
   * 清除已选择的文件
   */
  function clearFile() {
    pendingFile.value = null
  }

  /**
   * 执行文件上传（存储到 localStorage）
   * @returns {Object|null} 上传成功返回文件信息，失败返回 null
   */
  async function uploadFile() {
    if (!pendingFile.value) {
      ElMessage.error('请先选择文件')
      return null
    }

    uploadLoading.value = true

    try {
      // 读取文件内容
      const content = await readFileContent(pendingFile.value)
      const originalName = pendingFile.value.name
      const fileId = generateId('doc')

      // 存储到 localStorage
      const files = getStoredFiles()
      files[fileId] = {
        fileName: originalName,
        content: content,
        createdAt: new Date().toISOString()
      }
      setStoredFiles(files)

      return {
        fileId: fileId,
        fileName: originalName,
        url: `local://${fileId}`
      }
    } catch (error) {
      ElMessage.error('文件上传失败: ' + error.message)
      return null
    } finally {
      uploadLoading.value = false
    }
  }

  /**
   * 读取文件内容
   */
  function readFileContent(file) {
    return new Promise((resolve, reject) => {
      const reader = new FileReader()
      reader.onload = (e) => resolve(e.target.result)
      reader.onerror = (e) => reject(new Error('读取文件失败'))
      reader.readAsText(file)
    })
  }

  /**
   * 重置状态
   */
  function reset() {
    pendingFile.value = null
    uploadLoading.value = false
  }

  return {
    pendingFile,
    uploadLoading,
    handleFileSelect,
    clearFile,
    uploadFile,
    reset
  }
}

/**
 * 读取已上传的文件内容
 * @param {string} fileId - 文件 ID
 * @returns {string|null} 文件内容
 */
export function readFile(fileId) {
  const files = getStoredFiles()
  return files[fileId]?.content || null
}

/**
 * 删除已上传的文件
 * @param {string} fileId - 文件 ID
 */
export function deleteFile(fileId) {
  const files = getStoredFiles()
  delete files[fileId]
  setStoredFiles(files)
}

/**
 * 获取所有已上传的文件列表
 * @returns {Array} 文件列表
 */
export function listFiles() {
  const files = getStoredFiles()
  return Object.entries(files).map(([fileId, data]) => ({
    fileId,
    fileName: data.fileName,
    createdAt: data.createdAt,
    url: `local://${fileId}`
  }))
}

export default useFileUpload
