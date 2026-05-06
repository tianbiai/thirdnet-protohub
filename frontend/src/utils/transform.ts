/**
 * Key 转换工具 - 处理后端 snake_case 与前端 camelCase 的互转
 */

/** snake_case → camelCase */
export function snakeToCamel(str: string): string {
  return str.replace(/_([a-z])/g, (_, letter: string) => letter.toUpperCase())
}

/** camelCase → snake_case */
export function camelToSnake(str: string): string {
  return str.replace(/[A-Z]/g, letter => `_${letter.toLowerCase()}`)
}

/** 递归转换对象的所有 key */
function transformObjectKeys(
  obj: unknown,
  convertFn: (str: string) => string
): unknown {
  if (obj === null || obj === undefined) return obj
  if (typeof obj !== 'object') return obj
  if (obj instanceof Date) return obj
  if (obj instanceof File) return obj

  if (Array.isArray(obj)) {
    return obj.map(item => transformObjectKeys(item, convertFn))
  }

  const result: Record<string, unknown> = {}
  for (const key of Object.keys(obj as Record<string, unknown>)) {
    const newKey = convertFn(key)
    result[newKey] = transformObjectKeys(
      (obj as Record<string, unknown>)[key],
      convertFn
    )
  }
  return result
}

/** 将对象的所有 key 从 snake_case 转为 camelCase */
export function transformKeysToCamel<T = unknown>(obj: unknown): T {
  return transformObjectKeys(obj, snakeToCamel) as T
}

/** 将对象的所有 key 从 camelCase 转为 snake_case */
export function transformKeysToSnake<T = unknown>(obj: unknown): T {
  return transformObjectKeys(obj, camelToSnake) as T
}
