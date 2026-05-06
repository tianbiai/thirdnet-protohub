/**
 * 签名工具 - 用于生成 HMAC-SHA512 Basic 认证头
 *
 * 认证流程：
 * 1. 对 URL 相对路径（含时间戳）进行 HMAC-SHA512 加密
 * 2. 对结果进行 base64 编码得到密码 code
 * 3. 组成 application:code 格式再 base64 编码得到 Basic 头
 */

import CryptoJS from 'crypto-js'
import { APP_APPLICATION, APP_PREKEY, APP_AUTH_KEY } from '@/config'

/** 安全的 Base64 编码，支持非 ASCII 字符 */
function safeBase64(str: string): string {
  return btoa(encodeURIComponent(str).replace(/%([0-9A-F]{2})/g, (_, p1) => String.fromCharCode(parseInt(p1, 16))))
}

/** 获取完整密钥（prekey + key） */
export function getFullKey(key: string): string {
  return APP_PREKEY + key
}

/** 生成 HMAC-SHA512 签名并返回 base64 编码 */
export function hmacSha512(url: string, fullKey: string): string {
  const signature = CryptoJS.HmacSHA512(url, fullKey)
  return CryptoJS.enc.Base64.stringify(signature)
}

/** 生成 HMAC-SHA512 签名的 Basic 认证头 */
export function generateBasicAuth(
  url: string,
  key: string = APP_AUTH_KEY,
  timestamp: string | number
): string {
  const urlWithTimestamp = url.includes('?')
    ? `${url}&timestamp=${timestamp}`
    : `${url}?timestamp=${timestamp}`

  const fullKey = getFullKey(key)
  const code = hmacSha512(urlWithTimestamp, fullKey)
  const basicRaw = `${APP_APPLICATION}:${code}`
  return 'Basic ' + safeBase64(basicRaw)
}

/** 生成简单的 Basic 认证头（不带签名） */
export function generateSimpleBasic(application: string, password: string = ''): string {
  const basicRaw = `${application}:${password}`
  return 'Basic ' + safeBase64(basicRaw)
}

/** 获取 application 标识 */
export function getApplication(): string {
  return APP_APPLICATION
}

/** 获取当前时间戳（秒） */
export function getTimestamp(): number {
  return Math.floor(Date.now() / 1000)
}
