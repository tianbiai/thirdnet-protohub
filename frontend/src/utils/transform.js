/**
 * Key 转换工具 - 处理后端 snake_case 与前端 camelCase 的互转
 */

/**
 * snake_case → camelCase
 * @param {string} str
 * @returns {string}
 */
export function snakeToCamel(str) {
  return str.replace(/_([a-z])/g, (_, letter) => letter.toUpperCase())
}

/**
 * camelCase → snake_case
 * @param {string} str
 * @returns {string}
 */
export function camelToSnake(str) {
  return str.replace(/[A-Z]/g, letter => `_${letter.toLowerCase()}`)
}

/**
 * 递归转换对象的所有 key
 * @param {any} obj - 要转换的对象
 * @param {function} convertFn - 转换函数 (snakeToCamel | camelToSnake)
 * @returns {any}
 */
function transformObjectKeys(obj, convertFn) {
  if (obj === null || obj === undefined) return obj
  if (typeof obj !== 'object') return obj
  if (obj instanceof Date) return obj
  if (obj instanceof File) return obj

  if (Array.isArray(obj)) {
    return obj.map(item => transformObjectKeys(item, convertFn))
  }

  const result = {}
  for (const key of Object.keys(obj)) {
    const newKey = convertFn(key)
    result[newKey] = transformObjectKeys(obj[key], convertFn)
  }
  return result
}

/**
 * 将对象的所有 key 从 snake_case 转为 camelCase
 * @param {any} obj
 * @returns {any}
 */
export function transformKeysToCamel(obj) {
  return transformObjectKeys(obj, snakeToCamel)
}

/**
 * 将对象的所有 key 从 camelCase 转为 snake_case
 * @param {any} obj
 * @returns {any}
 */
export function transformKeysToSnake(obj) {
  return transformObjectKeys(obj, camelToSnake)
}
