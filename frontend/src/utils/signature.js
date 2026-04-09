/**
 * 签名工具 - 用于生成 Basic 认证头
 *
 * Basic 认证流程：
 * 1. 对 URL 相对路径（含时间戳）进行 HMAC-SHA512 加密
 * 2. 对结果进行 base64 编码得到密码 code
 * 3. 组成 application:code 格式
 * 4. 再进行 base64 编码得到 Basic 头
 *
 * 注意：密钥从环境配置中获取，不要在前端代码中硬编码
 */

import CryptoJS from 'crypto-js'

/**
 * 应用配置 - 从环境变量中读取
 */
const APP_CONFIG = {
  application: import.meta.env.VITE_APP_APPLICATION || '',
  prekey: import.meta.env.VITE_APP_PREKEY || ''
}

// 加密 key - 从环境变量中读取
const AUTH_KEY = import.meta.env.VITE_APP_AUTH_KEY || ''

/**
 * 获取完整密钥
 * @param {string} key - 从服务端获取的 key
 * @returns {string} 完整密钥 = UTF8(prekey + key)
 */
export function getFullKey(key) {
  return APP_CONFIG.prekey + key
}

/**
 * 生成 HMAC-SHA512 签名并返回 base64 编码
 * @param {string} url - URL 相对路径（含时间戳参数）
 * @param {string} fullKey - 完整密钥
 * @returns {string} base64 编码的签名
 */
export function hmacSha512(url, fullKey) {
  const signature = CryptoJS.HmacSHA512(url, fullKey)
  return CryptoJS.enc.Base64.stringify(signature)
}

/**
 * 生成 Basic 认证头
 * @param {string} url - URL 相对路径（不含时间戳）
 * @param {string} key - 加密 key（默认使用 AUTH_KEY）
 * @param {string|number} timestamp - 时间戳（秒）
 * @returns {string} Basic 认证头值
 */
export function generateBasicAuth(url, key = AUTH_KEY, timestamp) {
  // 添加时间戳参数
  const urlWithTimestamp = url.includes('?')
    ? `${url}&timestamp=${timestamp}`
    : `${url}?timestamp=${timestamp}`

  // 获取完整密钥
  const fullKey = getFullKey(key)

  // 生成密码 code
  const code = hmacSha512(urlWithTimestamp, fullKey)

  // 组成 Basic 原文：application:code
  const basicRaw = `${APP_CONFIG.application}:${code}`

  // base64 编码
  return 'Basic ' + btoa(basicRaw)
}

/**
 * 生成简单的 Basic 认证头（不带签名）
 * @param {string} application - 应用标识
 * @param {string} password - 密码
 * @returns {string} Basic 认证头值
 */
export function generateSimpleBasic(application, password = '') {
  const basicRaw = `${application}:${password}`
  return 'Basic ' + btoa(basicRaw)
}

/**
 * 获取 application
 * @returns {string}
 */
export function getApplication() {
  return APP_CONFIG.application
}

/**
 * 获取当前时间戳（秒）
 * @returns {number}
 */
export function getTimestamp() {
  return Math.floor(Date.now() / 1000)
}

export default {
  getFullKey,
  hmacSha512,
  generateBasicAuth,
  generateSimpleBasic,
  getApplication,
  getTimestamp
}
