# ProtoHub API 设计文档

> 版本：1.0.1
> 更新日期：2026-03-21
> 基于前端规格说明书和代码分析生成

---

## 1. 概述

### 1.1 文档说明

本文档定义了 ProtoHub（原型视界）后端 API 接口规范，供后端开发人员参考实现。

### 1.2 基础信息

| 项目 | 说明 |
|-----|------|
| 基础路径 | `/api` |
| 协议 | HTTPS |
| 数据格式 | JSON |
| 字符编码 | UTF-8 |
| 认证方式 | JWT (RSA) |

### 1.3 API 模块划分

| 模块 | 路由前缀 | 说明 |
|-----|---------|------|
| 用户认证 | `/api/app/user` | 登录、登出、获取当前用户 |
| 菜单查询 | `/api/app/menu` | 获取菜单配置（所有用户） |
| 菜单管理 | `/api/manager/menu` | 菜单增删改查（管理员） |

---

## 2. 通用规范

### 2.1 请求头

```
Content-Type: application/json
Accept: application/json
Authorization: Bearer <token>  // 需认证的接口
```

### 2.2 响应格式

**成功响应（HTTP 200）**

响应体直接返回业务数据，不包装：

```json
{
  "id": 1,
  "name": "示例数据"
}
```

**错误响应**

通过 HTTP 状态码标识错误类型，响应体返回错误详情：

```json
{
  "code": "ERROR_CODE",
  "message": "错误描述信息"
}
```

### 2.3 HTTP 状态码

| 状态码 | 说明 | 使用场景 |
|-------|------|---------|
| 200 | 成功 | 请求处理成功 |
| 400 | 请求参数错误 | 参数验证失败、格式错误 |
| 401 | 未认证 | Token 无效或已过期 |
| 403 | 无权限 | 已认证但权限不足 |
| 404 | 资源不存在 | 请求的资源未找到 |
| 409 | 资源冲突 | 名称重复等冲突场景 |
| 500 | 服务器内部错误 | 系统异常 |

### 2.4 错误码定义

| 错误码 | HTTP 状态码 | 说明 |
|-------|------------|------|
| `INVALID_CREDENTIALS` | 401 | 用户名或密码错误 |
| `TOKEN_EXPIRED` | 401 | Token 已过期 |
| `TOKEN_INVALID` | 401 | Token 无效 |
| `PERMISSION_DENIED` | 403 | 无权限访问 |
| `RESOURCE_NOT_FOUND` | 404 | 资源不存在 |
| `VALIDATION_ERROR` | 400 | 参数验证失败 |
| `DUPLICATE_NAME` | 409 | 名称重复 |

---

## 3. 用户认证模块

### 3.1 用户登录

用户通过用户名和密码登录系统。

**请求**

```
POST /api/app/user/login
```

**请求体**

```json
{
  "username": "admin",
  "password": "admin123"
}
```

| 字段 | 类型 | 必填 | 说明 |
|-----|------|-----|------|
| username | string | 是 | 用户名 |
| password | string | 是 | 密码（明文传输，建议 HTTPS） |

**成功响应** `200`

```json
{
  "token": "eyJhbGciOiJSUzI1NiIs...",
  "userInfo": {
    "id": 1,
    "username": "admin",
    "nickname": "管理员",
    "role": "admin"
  }
}
```

**错误响应** `401`

```json
{
  "code": "INVALID_CREDENTIALS",
  "message": "用户名或密码错误"
}
```

---

### 3.2 用户登出

退出当前登录状态。

**请求**

```
POST /api/app/user/logout
```

**请求头**

```
Authorization: Bearer <token>
```

**成功响应** `200`

```json
null
```

> 登出成功返回 `null`，客户端可忽略响应体

---

### 3.3 获取当前用户信息

获取当前登录用户的详细信息。

**请求**

```
POST /api/app/user/current
```

**请求头**

```
Authorization: Bearer <token>
```

**成功响应** `200`

```json
{
  "id": 1,
  "username": "admin",
  "nickname": "管理员",
  "role": "admin",
  "permissions": {
    "features": ["dashboard", "menu-editor", "projects", "project-detail", "content", "settings"],
    "canEditMenu": true
  }
}
```

**数据结构**

| 字段 | 类型 | 说明 |
|-----|------|------|
| id | long | 用户 ID |
| username | string | 用户名 |
| nickname | string | 昵称 |
| role | string | 角色（admin/guest） |
| permissions.features | string[] | 可访问的功能列表 |
| permissions.canEditMenu | boolean | 是否可编辑菜单 |

---

## 4. 菜单查询模块（应用端）

### 4.1 获取菜单配置

获取完整的菜单配置，包含所有分组和菜单项。

**请求**

```
POST /api/app/menu/list
```

**请求头**

```
Authorization: Bearer <token>
```

**成功响应** `200`

```json
{
  "title": "项目管理聚合基座",
  "version": "1.0.0",
  "groups": [
    {
      "id": 1,
      "name": "分组名称",
      "icon": "🔥",
      "order": 1,
      "children": [
        {
          "id": 1,
          "name": "菜单项名称",
          "icon": "📱",
          "type": "web",
          "url": "http://localhost:3000",
          "description": "描述信息",
          "order": 1,
          "viewport": {
            "width": 375,
            "height": 812
          },
          "docFileId": "doc-xxx",
          "docFileName": "文档名称.md",
          "docDescription": "文档描述",
          "route": "/page-path",
          "permission": null
        }
      ]
    }
  ]
}
```

---

## 5. 菜单管理模块（管理端）

> 所有接口需要管理员权限（role = admin），否则返回 403

### 5.1 分组管理

#### 5.1.1 获取分组列表

获取所有菜单分组列表。

**请求**

```
POST /api/manager/menu/group/list
```

**权限要求**

- admin

**成功响应** `200`

```json
[
  {
    "id": 1,
    "name": "燃气积分商城",
    "icon": "🔥",
    "order": 1,
    "itemCount": 2
  }
]
```

---

#### 5.1.2 创建分组

创建新的菜单分组。

**请求**

```
POST /api/manager/menu/group/create
```

**请求体**

```json
{
  "name": "新分组名称",
  "icon": "📁"
}
```

| 字段 | 类型 | 必填 | 说明 |
|-----|------|-----|------|
| name | string | 是 | 分组名称，最大 50 字符 |
| icon | string | 否 | 分组图标（emoji 或图片 URL），默认 📁 |

**成功响应** `200`

```json
{
  "id": 2,
  "name": "新分组名称",
  "icon": "📁",
  "order": 2
}
```

**错误响应** `409`

```json
{
  "code": "DUPLICATE_NAME",
  "message": "分组名称已存在"
}
```

---

#### 5.1.3 更新分组

更新指定分组的信息。

**请求**

```
POST /api/manager/menu/group/update
```

**请求体**

```json
{
  "id": 1,
  "name": "更新后的名称",
  "icon": "🔥"
}
```

| 字段 | 类型 | 必填 | 说明 |
|-----|------|-----|------|
| id | long | 是 | 分组 ID |
| name | string | 否 | 分组名称 |
| icon | string | 否 | 分组图标 |

**成功响应** `200`

```json
{
  "id": 1,
  "name": "更新后的名称",
  "icon": "🔥",
  "order": 1
}
```

**错误响应** `404`

```json
{
  "code": "RESOURCE_NOT_FOUND",
  "message": "分组不存在"
}
```

---

#### 5.1.4 删除分组

删除指定分组及其下所有菜单项。

**请求**

```
POST /api/manager/menu/group/delete
```

**请求体**

```json
{
  "id": 1
}
```

| 字段 | 类型 | 必填 | 说明 |
|-----|------|-----|------|
| id | long | 是 | 分组 ID |

**成功响应** `200`

```json
null
```

**错误响应** `404`

```json
{
  "code": "RESOURCE_NOT_FOUND",
  "message": "分组不存在"
}
```

---

#### 5.1.5 分组排序

批量更新分组的排序顺序。

**请求**

```
POST /api/manager/menu/group/reorder
```

**请求体**

```json
{
  "orders": [
    { "id": 1, "order": 1 },
    { "id": 2, "order": 2 },
    { "id": 3, "order": 3 }
  ]
}
```

| 字段 | 类型 | 必填 | 说明 |
|-----|------|-----|------|
| orders | array | 是 | 排序数据数组 |
| orders[].id | long | 是 | 分组 ID |
| orders[].order | int | 是 | 排序序号 |

**成功响应** `200`

```json
null
```

---

### 5.2 菜单项管理

#### 5.2.1 获取分组下的菜单项列表

获取指定分组下的所有菜单项。

**请求**

```
POST /api/manager/menu/item/list
```

**请求体**

```json
{
  "groupId": 1
}
```

| 字段 | 类型 | 必填 | 说明 |
|-----|------|-----|------|
| groupId | long | 是 | 分组 ID |

**成功响应** `200`

```json
[
  {
    "id": 1,
    "groupId": 1,
    "name": "小程序端",
    "icon": "📱",
    "type": "miniprogram",
    "url": "http://example.com/",
    "description": "描述信息",
    "order": 1,
    "viewport": { "width": 375, "height": 812 },
    "docFileId": null,
    "docFileName": null,
    "docDescription": null,
    "route": null,
    "permission": null
  }
]
```

---

#### 5.2.2 创建菜单项

在指定分组下创建新的菜单项。

**请求**

```
POST /api/manager/menu/item/create
```

**请求体**

```json
{
  "groupId": 1,
  "name": "小程序端",
  "icon": "📱",
  "type": "miniprogram",
  "url": "http://example.com/",
  "description": "描述信息",
  "viewport": { "width": 375, "height": 812 },
  "docFileId": null,
  "docFileName": null,
  "docDescription": null,
  "route": null,
  "permission": null
}
```

| 字段 | 类型 | 必填 | 说明 |
|-----|------|-----|------|
| groupId | long | 是 | 所属分组 ID |
| name | string | 是 | 菜单项名称，最大 100 字符 |
| icon | string | 否 | 图标（emoji 或图片 URL） |
| type | string | 是 | 类型：`web` / `miniprogram` / `doc` / `swagger` / `internal` |
| url | string | 条件 | 外部链接地址（type 为 web/miniprogram/swagger 时必填） |
| description | string | 否 | 描述信息 |
| viewport | object | 否 | 视口配置（type 为 miniprogram 时使用） |
| viewport.width | int | 否 | 视口宽度 |
| viewport.height | int | 否 | 视口高度 |
| docFileId | string | 否 | 文档文件 ID（type 为 doc 时使用） |
| docFileName | string | 否 | 文档文件名 |
| docDescription | string | 否 | 文档描述 |
| route | string | 否 | 内部路由路径（type 为 internal 时使用） |
| permission | string | 否 | 访问所需权限标识 |

**成功响应** `200`

```json
{
  "id": 1,
  "groupId": 1,
  "name": "小程序端",
  "icon": "📱",
  "type": "miniprogram",
  "url": "http://example.com/",
  "description": "描述信息",
  "order": 1
}
```

**错误响应** `400`

```json
{
  "code": "VALIDATION_ERROR",
  "message": "type 为 web/miniprogram/swagger 时 url 不能为空"
}
```

---

#### 5.2.3 更新菜单项

更新指定菜单项的信息。

**请求**

```
POST /api/manager/menu/item/update
```

**请求体**

```json
{
  "id": 1,
  "name": "更新后的名称",
  "icon": "🖥️",
  "type": "web",
  "url": "http://example.com/new/",
  "description": "更新后的描述",
  "viewport": null,
  "docFileId": null,
  "docFileName": null,
  "docDescription": null,
  "route": null,
  "permission": "admin"
}
```

| 字段 | 类型 | 必填 | 说明 |
|-----|------|-----|------|
| id | long | 是 | 菜单项 ID |
| ... | ... | ... | 其他字段同创建接口，均为可选 |

**成功响应** `200`

```json
{
  "id": 1,
  "groupId": 1,
  "name": "更新后的名称",
  "icon": "🖥️",
  "type": "web",
  "url": "http://example.com/new/",
  "description": "更新后的描述",
  "order": 1
}
```

---

#### 5.2.4 删除菜单项

删除指定的菜单项。

**请求**

```
POST /api/manager/menu/item/delete
```

**请求体**

```json
{
  "id": 1
}
```

| 字段 | 类型 | 必填 | 说明 |
|-----|------|-----|------|
| id | long | 是 | 菜单项 ID |

**成功响应** `200`

```json
null
```

---

#### 5.2.5 菜单项排序

批量更新同一分组下菜单项的排序顺序。

**请求**

```
POST /api/manager/menu/item/reorder
```

**请求体**

```json
{
  "groupId": 1,
  "orders": [
    { "id": 1, "order": 1 },
    { "id": 2, "order": 2 },
    { "id": 3, "order": 3 }
  ]
}
```

| 字段 | 类型 | 必填 | 说明 |
|-----|------|-----|------|
| groupId | long | 是 | 分组 ID |
| orders | array | 是 | 排序数据数组 |
| orders[].id | long | 是 | 菜单项 ID |
| orders[].order | int | 是 | 排序序号 |

**成功响应** `200`

```json
null
```

---

## 6. 数据模型

### 6.1 User（用户）

| 字段 | 类型 | 约束 | 说明 |
|------|------|------|------|
| Id | long | PK, Auto | 主键 |
| Username | string(50) | UK, Not Null | 用户名 |
| Password | string(256) | Not Null | 密码（加密存储） |
| Nickname | string(50) | Not Null | 昵称 |
| Role | string(20) | Not Null, Default: 'guest' | 角色 |
| Description | string(500) | Nullable | 描述 |
| CreatedAt | DateTime | Not Null | 创建时间 |
| UpdatedAt | DateTime | Not Null | 更新时间 |

**角色枚举**

| 值 | 说明 |
|---|------|
| admin | 管理员，完全权限 |
| guest | 访客，只读权限 |

---

### 6.2 MenuGroup（菜单分组）

| 字段 | 类型 | 约束 | 说明 |
|------|------|------|------|
| Id | long | PK, Auto | 主键 |
| Name | string(50) | Not Null | 分组名称 |
| Icon | string(100) | Nullable | 图标 |
| Order | int | Not Null, Default: 0 | 排序序号 |
| CreatedAt | DateTime | Not Null | 创建时间 |
| UpdatedAt | DateTime | Not Null | 更新时间 |

---

### 6.3 MenuItem（菜单项）

| 字段 | 类型 | 约束 | 说明 |
|------|------|------|------|
| Id | long | PK, Auto | 主键 |
| GroupId | long | FK, Not Null | 所属分组 ID |
| Name | string(100) | Not Null | 菜单项名称 |
| Icon | string(100) | Nullable | 图标 |
| Type | string(20) | Not Null | 类型 |
| Url | string(500) | Nullable | 外部链接 |
| Description | string(500) | Nullable | 描述 |
| Order | int | Not Null, Default: 0 | 排序序号 |
| Viewport | jsonb | Nullable | 视口配置 |
| DocFileId | string(100) | Nullable | 文档文件 ID |
| DocFileName | string(200) | Nullable | 文档文件名 |
| DocDescription | string(500) | Nullable | 文档描述 |
| Route | string(200) | Nullable | 内部路由路径 |
| Permission | string(100) | Nullable | 所需权限 |
| CreatedAt | DateTime | Not Null | 创建时间 |
| UpdatedAt | DateTime | Not Null | 更新时间 |

**类型枚举**

| 值 | 说明 |
|---|------|
| web | Web 应用 |
| miniprogram | 小程序/移动端 |
| doc | Markdown 文档 |
| swagger | API 文档 |
| internal | 内部路由 |

**Viewport JSON 结构**

```json
{
  "width": 375,
  "height": 812
}
```

---

### 6.4 ER 关系图

```
┌─────────────┐       ┌─────────────────┐
│    User     │       │   MenuGroup     │
├─────────────┤       ├─────────────────┤
│ Id (PK)     │       │ Id (PK)         │
│ Username    │       │ Name            │
│ Password    │       │ Icon            │
│ Nickname    │       │ Order           │
│ Role        │       │ CreatedAt       │
│ Description │       │ UpdatedAt       │
│ CreatedAt   │       └────────┬────────┘
│ UpdatedAt   │                │ 1
└─────────────┘                │
                               │
                               │ N
                      ┌────────┴────────┐
                      │    MenuItem     │
                      ├─────────────────┤
                      │ Id (PK)         │
                      │ GroupId (FK)    │
                      │ Name            │
                      │ Icon            │
                      │ Type            │
                      │ Url             │
                      │ Description     │
                      │ Order           │
                      │ Viewport        │
                      │ DocFileId       │
                      │ DocFileName     │
                      │ DocDescription  │
                      │ Route           │
                      │ Permission      │
                      │ CreatedAt       │
                      │ UpdatedAt       │
                      └─────────────────┘
```

---

## 7. 权限配置

### 7.1 角色权限映射

| 角色 | 功能权限 | 说明 |
|-----|---------|------|
| admin | dashboard, menu-editor, projects, project-detail, content, settings | 完全权限 |
| guest | dashboard, projects, project-detail, content | 只读权限 |

### 7.2 API 权限要求

| API | 角色要求 | 说明 |
|-----|---------|------|
| /api/app/user/login | 无需认证 | 登录接口 |
| /api/app/user/logout | 已认证 | 登出接口 |
| /api/app/user/current | 已认证 | 获取当前用户 |
| /api/app/menu/list | 已认证 | 获取菜单 |
| /api/manager/menu/* | admin | 管理接口 |

---

## 8. 缓存策略

### 8.1 菜单缓存

| 缓存键 | 类型 | 过期时间 | 说明 |
|-------|------|---------|------|
| `protohub:menu:config` | String | 5 分钟 | 完整菜单配置 |

**缓存失效策略**

- 创建/更新/删除分组时清除缓存
- 创建/更新/删除菜单项时清除缓存
- 排序操作时清除缓存

---

## 9. API 接口汇总

| 方法 | 路径 | 说明 | 权限 |
|-----|------|------|------|
| POST | /api/app/user/login | 用户登录 | 无 |
| POST | /api/app/user/logout | 用户登出 | 已认证 |
| POST | /api/app/user/current | 获取当前用户 | 已认证 |
| POST | /api/app/menu/list | 获取菜单配置 | 已认证 |
| POST | /api/manager/menu/group/list | 获取分组列表 | admin |
| POST | /api/manager/menu/group/create | 创建分组 | admin |
| POST | /api/manager/menu/group/update | 更新分组 | admin |
| POST | /api/manager/menu/group/delete | 删除分组 | admin |
| POST | /api/manager/menu/group/reorder | 分组排序 | admin |
| POST | /api/manager/menu/item/list | 获取菜单项列表 | admin |
| POST | /api/manager/menu/item/create | 创建菜单项 | admin |
| POST | /api/manager/menu/item/update | 更新菜单项 | admin |
| POST | /api/manager/menu/item/delete | 删除菜单项 | admin |
| POST | /api/manager/menu/item/reorder | 菜单项排序 | admin |

---

## 10. 前端适配说明

### 10.1 现有前端改造要点

当前前端使用 localStorage 和静态 JSON 文件存储数据，接入后端 API 后需要修改：

| 文件 | 改造内容 |
|-----|---------|
| `stores/user.js` | login 方法改为调用 API，移除静态文件读取 |
| `stores/menu.js` | 添加 API 调用方法，移除 localStorage 持久化逻辑 |
| `components/MenuEditor/*` | 调用后端 API 进行 CRUD 操作 |

### 10.2 API 请求封装示例

```javascript
// 封装 API 请求
async function apiCall(url, data) {
  const token = localStorage.getItem('token')
  const response = await fetch(url, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { 'Authorization': `Bearer ${token}` } : {})
    },
    body: JSON.stringify(data)
  })

  // 根据 HTTP 状态码判断成功/失败
  if (!response.ok) {
    const error = await response.json()
    throw new Error(error.message)
  }

  // 200 直接返回数据
  return response.json()
}

// 使用示例
try {
  const result = await apiCall('/api/app/user/login', {
    username: 'admin',
    password: 'admin123'
  })
  console.log('登录成功:', result.token)
} catch (error) {
  console.error('登录失败:', error.message)
}
```

---

## 11. 附录

### 11.1 默认用户数据

| ID | 用户名 | 密码 | 昵称 | 角色 |
|----|-------|------|------|------|
| 1 | admin | admin123 | 管理员 | admin |
| 2 | guest | guest123 | 访客 | guest |

### 11.2 菜单类型配置说明

| 类型 | 必填字段 | 可选字段 | 说明 |
|-----|---------|---------|------|
| web | url | description, permission | Web 应用，iframe 加载 |
| miniprogram | url | viewport, description | 小程序，模拟手机视口 |
| doc | docFileId, docFileName | docDescription | Markdown 文档 |
| swagger | url | description, permission | API 文档 |
| internal | route | permission | 内部路由页面 |

---

*文档结束*
