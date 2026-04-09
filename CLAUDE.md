# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 项目概述

ProtoHub（原型视界）是一个项目管理聚合平台，作为统一门户用于查看和管理各种 Web/移动应用程序及文档。通过动态菜单系统提供集中式界面访问不同项目。

### 架构模式

**单体服务架构** - 项目功能简单，所有业务模块（用户认证、菜单管理）合并到单一 API 服务中。

### 项目结构

```
protohub/
├── frontend/                    # 前端项目
│   └── protohub/               # Vue 3 应用
└── backend/                     # 后端项目
    └── ProtoHub/               # .NET 10 单体服务
        ├── ProtoHub.Api/       # API 服务（含认证+业务）
        │   ├── ProtoHub.Api/       # Web API 主项目
        │   └── ProtoHub.Database/  # 数据库层
        └── Tools/              # 工具库
            ├── ProtoHub.Cache/     # Redis 缓存
            └── ProtoHub.Common/    # 公共工具
```

---

## 前端 (Frontend)

### 技术栈

| 技术 | 版本 | 用途 |
|-----|------|------|
| Vue | 3.4 | 框架 (Composition API + `<script setup>`) |
| Vite | 5.x | 构建工具 |
| Element Plus | 2.x | UI 组件库 (中文 locale) |
| Pinia | - | 状态管理 |
| Vue Router | 4 | 路由 (hash history) |
| SCSS | - | 样式 (玻璃拟态设计系统) |

### 常用命令

```bash
cd frontend/protohub

npm install          # 安装依赖
npm run dev          # 启动开发服务器 (端口 3000)
npm run build        # 生产构建
npm run preview      # 预览生产构建
```

### 核心架构

#### 状态管理 (Pinia Stores)

| Store | 职责 |
|-------|------|
| `useAppStore` | 侧边栏折叠、主题切换 (light/dark)、面包屑 |
| `useMenuStore` | 菜单分组和项目 (从 menus.json/API 加载) |
| `useUserStore` | 认证、用户信息、角色权限 |

#### 认证系统

- 基于 JWT (RSA) 的认证，通过后端 `/connect/token` 获取令牌
- 前端 HMAC 签名密钥从环境变量 `VITE_APP_AUTH_KEY` 读取
- 两种角色：`admin` (完全权限) / `guest` (只读)

#### 路由守卫

- 保护路由的认证检查
- 已登录用户重定向离开登录页
- 角色访问控制 (`requireAdmin` meta)
- 权限访问控制 (`permission` meta)

#### 菜单类型

| 类型 | 说明 |
|-----|------|
| `web` | Web 应用 (iframe 显示) |
| `miniprogram` | 小程序/移动端 (自定义视口) |
| `link` | 超链接 (外部链接) |
| `doc` | Markdown 文档 |
| `internal` | 内部路由 |

#### 样式系统

- 基于 Apple 设计语言 + 玻璃拟态
- 通过 `[data-theme="dark"]` 支持明暗主题
- CSS 变量定义在 `variables.scss`

### 目录结构

```
frontend/protohub/src/
├── components/           # 可复用组件
│   ├── Layout/           # 主布局 (Sidebar, Header)
│   ├── DocViewer/        # Markdown 文档查看器
│   ├── IframeViewer/     # 内嵌页面查看器
│   ├── MenuEditor/       # 动态菜单配置
│   └── HelpBubble/       # 帮助气泡
├── composables/          # Vue 组合式函数
├── stores/               # Pinia 状态库
├── views/                # 路由视图
├── router/index.js       # 路由定义与守卫
├── styles/               # 全局样式与变量
├── utils/                # 工具函数
└── config/               # 配置文件
```

### 开发约定

- 使用 `@` 别名导入 (vite.config.js 配置)
- Element Plus 图标全局注册
- SCSS 预处理器自动注入 `variables.scss`
- 路由使用 hash history 兼容静态部署
- **必须使用 Composition API + `<script setup>`**

### Thirdnet Frontend 技能

| 技能 | 用途 | 触发条件 |
|-----|------|---------|
| `vue-best-practices` | Vue 3 最佳实践 | 所有 Vue.js 任务 |
| `vue-router-best-practices` | Vue Router 4 模式 | 路由相关 |
| `vue-pinia-best-practices` | Pinia 状态管理 | Store 相关 |
| `vue-jsx-best-practices` | Vue JSX 语法 | JSX 开发 |
| `create-adaptable-composable` | 创建可复用组合式函数 | Composable 开发 |
| `frontend-design` | 高质量前端界面 | UI/UX 开发 |
| `frontend` | 创建/修改前端页面组件 | 页面/组件开发 |

---

## 后端 (Backend)

### 技术栈

| 技术 | 版本 | 用途 |
|-----|------|------|
| .NET | 10 | 运行时框架 |
| ThirdNet.Core.AspNetCore | - | 三网核心框架 |
| PostgreSQL | - | 主数据库 |
| Redis | - | 缓存服务 |
| JWT (RSA) | - | 认证授权 |

### 常用命令

```bash
cd backend/ProtoHub

# 还原依赖
dotnet restore

# 运行 API 服务
dotnet run --project ProtoHub.Api/ProtoHub.Api

# 数据库迁移
dotnet ef migrations add InitialCreate --project ProtoHub.Api/ProtoHub.Database

# 发布
dotnet publish -c Release
```

### 服务架构

**单体服务模式** - 认证与业务功能合并，无需独立 Identity 服务。

| 服务 | 职责 |
|-----|------|
| ProtoHub.Api | 统一 API 服务，包含用户认证 + 菜单管理 + 所有业务逻辑 |

#### API 模块划分

| 模块 | 路由前缀 | 说明 |
|-----|---------|------|
| 用户认证 | `api/app/user/*` | 登录、登出、获取当前用户 |
| 菜单管理 | `api/app/menu/*` | 获取菜单列表 |
| 菜单管理(管理员) | `api/manager/menu/*` | 增删改菜单分组和项目 |

#### 数据模型

**User（用户）**

| 字段 | 类型 | 说明 |
|------|------|------|
| Id | long | 主键 |
| Username | string | 用户名 |
| Password | string | 密码（加密） |
| Role | string | 角色（admin/guest） |
| Permissions | string[] | 权限列表 |

**MenuGroup（菜单分组）**

| 字段 | 类型 | 说明 |
|------|------|------|
| Id | long | 主键 |
| Name | string | 分组名称 |
| Icon | string | 图标 |
| Order | int | 排序 |

**MenuItem（菜单项）**

| 字段 | 类型 | 说明 |
|------|------|------|
| Id | long | 主键 |
| GroupId | long | 分组 ID |
| Name | string | 菜单名称 |
| Type | string | 类型（web/miniprogram/link/doc/internal） |
| Path | string | 路径 |
| Permission | string | 所需权限 |
| Config | object | 配置（JSON） |

### 项目结构

```
backend/ProtoHub/
├── ProtoHub.slnx                    # 解决方案文件
├── ProtoHub.Api/
│   ├── ProtoHub.Api/                # Web API 主项目
│   │   ├── Controllers/             # 控制器
│   │   │   ├── App/                 # 应用接口
│   │   │   └── Manager/             # 管理接口
│   │   ├── Data/                    # 数据访问
│   │   │   └── Migrations/          # EF Core 迁移
│   │   ├── Map/                     # 对象映射
│   │   ├── Startup.cs               # 启动配置
│   │   └── appsettings.json         # 应用配置
│   └── ProtoHub.Database/           # 数据库层
│       ├── Models/                  # 实体模型
│       ├── Configurations/          # Fluent API 配置
│       └── ProtoHubDbContext.cs     # DbContext
└── Tools/
    ├── ProtoHub.Cache/              # Redis 缓存
    └── ProtoHub.Common/             # 公共工具
```

### 认证授权

- **认证方式**: JWT (RSA 公钥)
- **角色定义**:
  - `admin`: 完全权限
  - `guest`: 只读权限
- **权限控制**: 基于权限字符串的访问控制

### 配置说明

敏感配置（数据库连接字符串、RSA 密钥）通过环境变量提供。参考 `appsettings.Template.json` 获取配置模板。

```bash
# 必需的环境变量
PROTOHUB_DEFAULT_CONNECTION  # 配置库连接字符串
PROTOHUB_CONNECTION          # 主数据库连接字符串
PROTOHUB_JWT_PRIVATE_KEY     # RSA 私钥
PROTOHUB_JWT_PUBLIC_KEY      # RSA 公钥
```

```json
// appsettings.Template.json（配置模板）
{
  "DefaultConnectionString": "PostgreSQL 连接字符串",
  "ConnectionString": "PostgreSQL 连接字符串",
  "jwt": {
    "private_key": "RSA 私钥",
    "public_key": "RSA 公钥",
    "type": "RSA"
  }
}
```

### 密码安全

- 用户密码使用 BCrypt 哈希存储（work factor = 12）
- 种子管理员账户默认密码: `admin123`（BCrypt 哈希）
- 密码工具类: `ProtoHub.Api/Helpers/PasswordHelper.cs`

### Thirdnet Backend 技能

| 技能 | 用途 | 触发条件 |
|-----|------|---------|
| `backend` | 创建/修改后端 API 和服务 | 后端开发 |
| `net-api-developer` | .NET API 接口开发 | Controller/路由/端点 |
| `net-efcore-developer` | EF Core 数据库开发 | 实体/DbContext/迁移 |
| `net-cache-use` | Redis 缓存功能开发 | 缓存/字典/配置 |
| `net-microservice-generator` | .NET 微服务生成 | 新项目/解决方案 |
| `net-background-job` | 后台定时任务 | 定时/周期/后台任务 |
| `net-database-bulkcopy` | PostgreSQL 批量操作 | 批量/导入/同步 |

---

## 配置文件索引

| 文件 | 说明 |
|-----|------|
| `frontend/protohub/src/styles/variables.scss` | CSS 设计令牌 |
| `frontend/protohub/vite.config.js` | Vite 构建配置 |
| `backend/ProtoHub/ProtoHub.Api/ProtoHub.Api/appsettings.Template.json` | 后端应用配置模板 |
| `backend/ProtoHub/ProtoHub.Api/ProtoHub.Api/appsettings.json` | 后端应用配置（不入版本库， |

---

## 开发约定

### 通用约定

- 代码提交前确保构建通过
- 遵循现有代码风格和命名规范

### 前端约定

- 必须使用 Composition API + `<script setup>` + TypeScript
- 组件开发时参考 `vue-best-practices` 技能
- 样式使用 CSS 变量保持主题一致性

### 后端约定

- 遵循 ThirdNet.Core.AspNetCore 框架规范
- API 接口使用 POST 方法
- 数据库操作通过 EF Core
- 密码使用 BCrypt 哈希（`PasswordHelper` 工具类）
- 时间戳由 `ProtoHubDbContext.UpdateTimestamps` 自动管理，控制器中无需手动设置
- 所有管理端接口必须添加 `[HasPermission]` 权限校验
- DTO 类必须添加 `[Required]`、`[StringLength]`、`[Range]` 等验证注解
- 敏感配置通过环境变量提供，不硬编码在源码中
