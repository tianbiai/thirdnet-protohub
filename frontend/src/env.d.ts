/// <reference types="vite/client" />

declare module '*.vue' {
  import type { DefineComponent } from 'vue'
  const component: DefineComponent<object, object, unknown>
  export default component
}

/** Vite 环境变量类型声明 */
interface ImportMetaEnv {
  /** API 基础地址 */
  readonly VITE_API_BASE_URL: string
  /** 是否启用 Mock 模式 */
  readonly VITE_MOCK: string
  /** 应用标识 */
  readonly VITE_APP_APPLICATION: string
  /** HMAC 签名前缀 */
  readonly VITE_APP_PREKEY: string
  /** HMAC 签名密钥 */
  readonly VITE_APP_AUTH_KEY: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
