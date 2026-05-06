/**
 * 应用全局配置
 * 从环境变量读取，构建时由 Vite 注入
 */

/** 是否启用 Mock 模式（通过 .env 中 VITE_MOCK 切换） */
export const MOCK_ENABLED = import.meta.env.VITE_MOCK === 'true'

/** API 基础地址 */
export const API_BASE_URL: string = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:6003'

/** 应用标识（用于 HMAC 签名） */
export const APP_APPLICATION: string = import.meta.env.VITE_APP_APPLICATION ?? ''

/** HMAC 签名前缀 */
export const APP_PREKEY: string = import.meta.env.VITE_APP_PREKEY ?? ''

/** HMAC 签名密钥 */
export const APP_AUTH_KEY: string = import.meta.env.VITE_APP_AUTH_KEY ?? ''
