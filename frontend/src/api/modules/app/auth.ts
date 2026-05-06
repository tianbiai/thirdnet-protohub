/**
 * 认证模块 API
 *
 * 提供用户登录、登出、令牌刷新、获取当前用户信息、修改密码等接口。
 * 登录和刷新令牌使用表单编码（application/x-www-form-urlencoded），
 * 其余接口使用标准 JSON 格式。
 */

import { request } from '@/api/request'
import { MOCK_ENABLED } from '@/config'

// ==================== 枚举 ====================

/** 用户角色枚举 */
export enum UserRole {
  /** 管理员 */
  Admin = 'admin',
  /** 访客 */
  Guest = 'guest'
}

// ==================== 类型定义 ====================

/** 登录请求参数 */
export interface LoginParams {
  /** 用户名 */
  username: string
  /** 密码 */
  password: string
  /** 权限范围（默认 'protohub offline_access'） */
  scope?: string
}

/** 令牌响应 */
export interface TokenResponse {
  /** 访问令牌 */
  access_token: string
  /** 刷新令牌 */
  refresh_token?: string
}

/** 角色信息 */
export interface RoleInfo {
  /** 角色编码 */
  code: string
  /** 角色名称 */
  name: string
}

/** 权限信息 */
export interface PermissionInfo {
  /** 权限编码 */
  code: string
}

/** 项目访问信息 */
export interface ProjectAccessInfo {
  /** 项目 ID */
  id: number
  /** 访问类型 */
  accessType: string
}

/** 当前用户响应 */
export interface CurrentUserResponse {
  /** 用户 ID */
  id: number
  /** 用户名 */
  userName: string
  /** 昵称 */
  nickName: string
  /** 角色列表 */
  roles: RoleInfo[]
  /** 权限列表 */
  permissions: PermissionInfo[]
  /** 可访问项目列表 */
  projects: ProjectAccessInfo[]
}

/** 修改密码请求参数 */
export interface ChangePasswordParams {
  /** 旧密码 */
  oldPassword: string
  /** 新密码 */
  newPassword: string
}

// ==================== 接口定义 ====================

/** 认证模块接口 */
export interface IAuthApi {
  /**
   * 用户登录
   * @param params - 登录参数
   * @returns 令牌响应（包含 access_token 和 refresh_token）
   */
  login(params: LoginParams): Promise<TokenResponse>

  /**
   * 刷新令牌
   * @param refreshToken - 刷新令牌
   * @returns 新的令牌响应
   */
  refreshToken(refreshToken: string): Promise<TokenResponse>

  /**
   * 用户登出
   */
  logout(): Promise<void>

  /**
   * 获取当前登录用户信息
   * @returns 当前用户信息
   */
  getCurrentUser(): Promise<CurrentUserResponse>

  /**
   * 修改密码
   * @param params - 修改密码参数
   */
  changePassword(params: ChangePasswordParams): Promise<void>
}

// ==================== Real 实现 ====================

/** 认证模块 - 真实 API 实现 */
export class RealAuthApi implements IAuthApi {
  /**
   * 用户登录
   * 使用表单编码格式发送请求，跳过认证重定向
   */
  async login(params: LoginParams): Promise<TokenResponse> {
    const formData = new URLSearchParams()
    formData.append('username', params.username)
    formData.append('password', params.password)
    formData.append('scope', params.scope || 'protohub offline_access')

    return request<TokenResponse>({
      url: '/connect/token',
      method: 'POST',
      data: formData.toString(),
      rawResponse: true,
      skipAuthRedirect: true,
      forceBasicAuth: true
    })
  }

  /**
   * 刷新令牌
   * 使用表单编码格式发送请求
   */
  async refreshToken(refreshToken: string): Promise<TokenResponse> {
    const formData = new URLSearchParams()
    formData.append('refresh_token', refreshToken)

    return request<TokenResponse>({
      url: '/connect/token/refresh',
      method: 'POST',
      data: formData.toString(),
      rawResponse: true,
      skipAuthRedirect: true,
      forceBasicAuth: true
    })
  }

  /**
   * 用户登出
   */
  async logout(): Promise<void> {
    return request<void>({
      url: '/api/app/auth/logout',
      method: 'POST'
    })
  }

  /**
   * 获取当前登录用户信息
   */
  async getCurrentUser(): Promise<CurrentUserResponse> {
    return request<CurrentUserResponse>({
      url: '/api/app/auth/me',
      method: 'POST'
    })
  }

  /**
   * 修改密码
   */
  async changePassword(params: ChangePasswordParams): Promise<void> {
    return request<void>({
      url: '/api/app/auth/change-password',
      method: 'POST',
      data: params
    })
  }
}

// ==================== Mock 实现 ====================

/** 认证模块 - Mock 实现 */
export class MockAuthApi implements IAuthApi {
  /**
   * 模拟登录
   * 返回模拟的令牌数据
   */
  async login(params: LoginParams): Promise<TokenResponse> {
    // 模拟网络延迟
    await new Promise(resolve => setTimeout(resolve, 500))

    // 模拟验证：用户名 admin / 密码 admin123
    if (params.username === 'admin' && params.password === 'admin123') {
      return {
        access_token: 'mock_access_token_' + Date.now(),
        refresh_token: 'mock_refresh_token_' + Date.now()
      }
    }

    // 模拟访客用户
    if (params.username === 'guest' && params.password === 'guest123') {
      return {
        access_token: 'mock_guest_token_' + Date.now(),
        refresh_token: 'mock_guest_refresh_' + Date.now()
      }
    }

    throw new Error('用户名或密码错误')
  }

  /**
   * 模拟刷新令牌
   */
  async refreshToken(refreshToken: string): Promise<TokenResponse> {
    await new Promise(resolve => setTimeout(resolve, 300))

    return {
      access_token: 'mock_refreshed_token_' + Date.now(),
      refresh_token: refreshToken
    }
  }

  /**
   * 模拟登出
   */
  async logout(): Promise<void> {
    await new Promise(resolve => setTimeout(resolve, 200))
  }

  /**
   * 模拟获取当前用户信息
   */
  async getCurrentUser(): Promise<CurrentUserResponse> {
    await new Promise(resolve => setTimeout(resolve, 300))

    return {
      id: 1,
      userName: 'admin',
      nickName: '系统管理员',
      roles: [
        { code: UserRole.Admin, name: '管理员' }
      ],
      permissions: [
        { code: 'menu:manage' },
        { code: 'user:manage' },
        { code: 'role:manage' },
        { code: 'project:manage' }
      ],
      projects: [
        { id: 1, accessType: 'full' },
        { id: 2, accessType: 'read' }
      ]
    }
  }

  /**
   * 模拟修改密码
   */
  async changePassword(params: ChangePasswordParams): Promise<void> {
    await new Promise(resolve => setTimeout(resolve, 300))

    if (params.oldPassword === 'wrong') {
      throw new Error('旧密码不正确')
    }
  }
}

// ==================== 工厂函数 ====================

/**
 * 创建认证 API 实例
 * 根据全局 Mock 开关返回真实或模拟实现
 * @returns 认证 API 实例
 */
export function createAuthApi(): IAuthApi {
  return MOCK_ENABLED ? new MockAuthApi() : new RealAuthApi()
}

// ==================== 单例导出 ====================

/** 认证模块 API 单例 */
export const authApi = createAuthApi()
