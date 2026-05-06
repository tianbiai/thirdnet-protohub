/**
 * 异步操作锁，防止并发调用
 * 支持全局锁和按 key 独立锁
 */
import { ref, reactive } from 'vue'

export function useAsyncLock() {
  /** 全局加载状态 */
  const loading = ref(false)

  /** 按 key 的加载状态映射 */
  const loadingMap = reactive<Record<string | number, boolean>>({})

  /** 全局锁包装函数 */
  function wrap<T extends (...args: unknown[]) => Promise<unknown>>(fn: T) {
    return async (...args: Parameters<T>) => {
      if (loading.value) return
      loading.value = true
      try {
        return await fn(...args)
      } finally {
        loading.value = false
      }
    }
  }

  /** 按指定 key 创建独立 loading 状态的包装函数 */
  function forKey<T extends (...args: unknown[]) => Promise<unknown>>(
    key: string | number,
    fn: T
  ) {
    return async (...args: Parameters<T>) => {
      if (loadingMap[key]) return
      loadingMap[key] = true
      try {
        return await fn(...args)
      } finally {
        loadingMap[key] = false
      }
    }
  }

  /** 获取指定 key 的 loading 状态 */
  function isLoading(key?: string | number): boolean {
    if (key !== undefined) return !!loadingMap[key]
    return loading.value
  }

  return reactive({ loading, wrap, forKey, isLoading })
}
