import { ref, reactive } from 'vue'

/**
 * 异步操作锁，防止并发调用
 * 支持全局锁和按 key 独立锁（用于表格行按钮等场景）
 */
export function useAsyncLock() {
  const loading = ref(false)
  const loadingMap = reactive({})

  function wrap(fn) {
    return async (...args) => {
      if (loading.value) return
      loading.value = true
      try {
        return await fn(...args)
      } finally {
        loading.value = false
      }
    }
  }

  /**
   * 按指定 key 创建独立 loading 状态的 wrap 函数
   * 适用于表格中每行按钮需要独立 loading 的场景
   */
  function forKey(key, fn) {
    return async (...args) => {
      if (loadingMap[key]) return
      loadingMap[key] = true
      try {
        return await fn(...args)
      } finally {
        loadingMap[key] = false
      }
    }
  }

  /**
   * 获取指定 key 的 loading 状态
   */
  function isLoading(key) {
    if (key !== undefined) return !!loadingMap[key]
    return loading.value
  }

  return reactive({ loading, wrap, forKey, isLoading })
}
