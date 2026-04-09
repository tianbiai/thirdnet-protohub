/**
 * API 类型定义
 * 基于后端 OpenAPI 规范生成
 */

// ==================== 通用类型 ====================

/** 分页请求 */
export interface PageRequest {
  page?: number
  pagesize?: number
}

/** 分页响应 */
export interface PageResponse<T> {
  list: T[]
  total: number
  page: number
  pagesize: number
}

/** 删除请求 */
export interface DeleteRequest {
  id: number
}

/** 排序项 */
export interface OrderItem {
  id: number
  order: number
}

// ==================== 认证相关 ====================

/** 登录请求 */
export interface LoginRequest {
  username: string
  password: string
}

/** 用户信息 */
export interface UserInfo {
  id: number
  username: string | null
  nickname: string | null
  email: string | null
  roles: string[] | null
  permissions: string[] | null
  accessibleprojects: number[] | null
  manageableprojects: number[] | null
}

/** 登录响应 */
export interface LoginResponse {
  token: string | null
  userinfo: UserInfo | null
}

/** 当前用户响应 */
export interface CurrentUserResponse extends UserInfo {}

/** 修改密码请求 */
export interface ChangePasswordRequest {
  oldpassword: string
  newpassword: string
}

// ==================== 菜单相关 ====================

/** 视口配置 */
export interface ViewportConfig {
  width?: number
  height?: number
  scalable?: boolean
  device?: string | null
}

/** 菜单项响应 */
export interface MenuItemResponse {
  id: number
  name: string | null
  type: string | null
  url: string | null
  description: string | null
  order: number
  viewport: ViewportConfig | null
  docfileid: string | null
  docfilename: string | null
  docdescription: string | null
  route: string | null
}

/** 菜单分组响应 */
export interface MenuGroupResponse {
  id: number
  name: string | null
  icon: string | null
  order: number
  children: MenuItemResponse[] | null
}

/** 菜单配置响应 */
export interface MenuConfigResponse {
  title: string | null
  version: string | null
  groups: MenuGroupResponse[] | null
}

/** 分组列表响应（管理端） */
export interface GroupListResponse extends MenuGroupResponse {
  itemcount: number
}

/** 菜单项详情响应（管理端） */
export interface MenuItemDetailResponse extends MenuItemResponse {
  groupid: number
}

// ==================== 菜单管理请求 ====================

/** 创建分组请求 */
export interface CreateGroupRequest {
  name: string
}

/** 更新分组请求 */
export interface UpdateGroupRequest {
  id: number
  name?: string | null
}

/** 分组排序请求 */
export interface ReorderRequest {
  orders: OrderItem[]
}

/** 菜单项列表请求 */
export interface ItemListRequest {
  groupid: number
}

/** 创建菜单项请求 */
export interface CreateItemRequest {
  groupid: number
  name: string
  type: string
  url?: string | null
  description?: string | null
  viewport?: ViewportConfig | null
  docfileid?: string | null
  docfilename?: string | null
  docdescription?: string | null
  route?: string | null
}

/** 更新菜单项请求 */
export interface UpdateItemRequest {
  id: number
  name?: string | null
  type?: string | null
  url?: string | null
  description?: string | null
  viewport?: ViewportConfig | null
  docfileid?: string | null
  docfilename?: string | null
  docdescription?: string | null
  route?: string | null
}

/** 菜单项排序请求 */
export interface ReorderItemsRequest {
  groupid: number
  orders: OrderItem[]
}

// ==================== 项目相关 ====================

/** 项目项 */
export interface ProjectItem {
  id: number
  name: string | null
  icon: string | null
  order: number
  description: string | null
}

/** 菜单项信息（项目详情中） */
export interface MenuItemInfo {
  id: number
  name: string | null
  type: string | null
  path: string | null
  permission: string | null
  order: number
}

/** 项目详情 */
export interface ProjectDetail extends ProjectItem {
  items: MenuItemInfo[] | null
}

// ==================== 权限相关 ====================

/** 权限项 */
export interface PermissionItem {
  id: number
  code: string | null
  name: string | null
  category: string | null
  description: string | null
}

/** 权限列表请求 */
export interface PermissionListRequest {
  category?: string | null
}

// ==================== 角色相关 ====================

/** 角色项 */
export interface RoleItem {
  id: number
  code: string | null
  name: string | null
  description: string | null
  issystem: boolean
  createtime: string
  updatetime: string
}

/** 角色详情 */
export interface RoleDetail extends RoleItem {
  permissions: PermissionItem[] | null
}

/** 角色列表请求 */
export interface RoleListRequest {
  name?: string | null
}

/** 创建角色请求 */
export interface CreateRoleRequest {
  code: string
  name: string
  description?: string | null
}

/** 更新角色请求 */
export interface UpdateRoleRequest {
  id: number
  name: string
  description?: string | null
}

// ==================== 用户相关 ====================

/** 用户项 */
export interface UserItem {
  id: number
  username: string | null
  nickname: string | null
  email: string | null
  status: number
  description: string | null
  roles: RoleItem[] | null
  createtime: string
  updatetime: string
}

/** 用户列表请求 */
export interface UserListRequest extends PageRequest {
  username?: string | null
  nickname?: string | null
  status?: number | null
  rolecode?: string | null
}

/** 创建用户请求 */
export interface CreateUserRequest {
  username: string
  password: string
  nickname: string
  email?: string | null
  description?: string | null
  roleids?: number[] | null
}

/** 更新用户请求 */
export interface UpdateUserRequest {
  id: number
  nickname: string
  email?: string | null
  status?: number | null
  description?: string | null
}

// ==================== 项目授权相关 ====================

/** 项目授权项 */
export interface ProjectAccessItem {
  id: number
  userid: number
  username: string | null
  usernickname: string | null
  projectid: number
  projectname: string | null
  accesstype: string | null
  grantedby: number | null
  grantedbyname: string | null
  createtime: string
}

/** 用户项目访问项 */
export interface UserProjectAccessItem {
  id: number
  projectid: number
  projectname: string | null
  projecticon: string | null
  accesstype: string | null
  createtime: string
}

/** 项目授权列表请求 */
export interface ProjectAccessListRequest extends PageRequest {
  projectid?: number | null
  userid?: number | null
  accesstype?: string | null
}

/** 授予项目访问权限请求 */
export interface GrantProjectAccessRequest {
  userid: number
  projectid: number
  accesstype: string
}

/** 撤销项目访问权限请求 */
export interface RevokeProjectAccessRequest {
  userid: number
  projectid: number
}
