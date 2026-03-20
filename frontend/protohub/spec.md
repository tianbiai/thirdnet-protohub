# ProtoHub 原型视界 - 规格说明书

## 1. 项目概述

### 1.1 项目名称
ProtoHub 原型视界（ProtoHub Prototype Vision）

### 1.2 项目描述
一个基于 Vue 3 的前端项目聚合管理平台，提供统一的项目预览、文档查看、菜单管理等功能。支持通过 iframe 加载多个子项目，实现多项目统一入口管理。

### 1.3 目标平台
- Web 端（桌面浏览器）
- 响应式设计，支持主流浏览器

### 1.4 设计理念
采用苹果公司设计理念（Apple Design Guidelines），核心原则：
- **简洁克制**：去除冗余元素，保持界面清爽
- **清晰层次**：通过留白、字体大小、颜色对比建立视觉层级
- **一致性**：统一的交互模式和视觉语言
- **深度与动效**：适度使用模糊、阴影和流畅动效增强体验

## 2. 技术栈

### 2.1 核心框架
| 技术 | 版本 | 用途 |
|-----|------|-----|
| Vue | 3.4.x | 前端框架 |
| Vite | 5.x | 构建工具 |
| Element Plus | 2.x | UI 组件库 |
| Vue Router | 4.x | 路由管理 |
| Pinia | 2.x | 状态管理 |
| Axios | 1.x | HTTP 请求 |

### 2.2 辅助库
| 技术 | 版本 | 用途 |
|-----|------|-----|
| marked | latest | Markdown 渲染 |
| highlight.js | latest | 代码高亮 |
| vuedraggable | 4.x | 拖拽排序 |

## 3. 功能模块

### 3.1 用户认证与权限管理
- **简单登录**：基于 localStorage 的简单认证
- **路由守卫**：未登录自动跳转登录页
- **角色权限**：
  - **admin（管理员）**：完全权限，可访问所有功能和系统管理菜单
  - **guest（访客）**：只读权限，只能查看项目菜单，无法访问系统管理菜单
- **默认账号**：
  - admin / admin123（管理员）
  - guest / guest123（访客）
- **菜单权限**：
  - admin：可看到仪表盘、菜单编辑器、项目列表、系统设置
  - guest：只能看到仪表盘和项目列表

### 3.2 主布局
- **顶部导航栏**：面包屑导航、帮助按钮、主题切换、用户菜单
- **侧边栏**：可折叠、分组菜单、状态持久化
- **内容区**：路由视图、页面切换动画

### 3.3 仪表盘
- **统计卡片**：项目总数、移动端项目、文档数量、菜单分组
- **快速入口**：最近访问的项目
- **系统信息**：版本、技术栈展示

### 3.4 菜单编辑器
- **可视化编辑**：左右分栏，编辑面板 + 实时预览
- **分组管理**：添加、编辑、删除、拖拽排序
- **菜单项管理**：添加、编辑、删除、拖拽排序
- **类型支持**：Web 应用、移动端应用、文档、内部页面
- **配置导出**：导出为 JSON 文件

### 3.5 项目列表
- **分组展示**：按分组显示项目列表
- **快速访问**：点击直接打开项目或文档

### 3.6 内容查看
- **iframe 查看器**：加载 Web 应用和移动端应用
- **移动端模拟**：模拟手机外观，支持自定义视口
- **文档查看器**：Markdown 渲染，代码高亮

### 3.7 系统设置
- **主题切换**：亮色/暗色主题
- **菜单管理**：重置、导出配置
- **用户信息**：显示当前用户信息

## 4. 数据结构

### 4.1 菜单配置（public/menus.json）
默认菜单配置文件，首次加载时读取并缓存到 localStorage。

```json
{
  "title": "项目管理聚合基座",
  "version": "1.0.0",
  "groups": [
    {
      "id": "group-001",
      "name": "分组名称",
      "icon": "🔥",
      "expanded": true,
      "children": [
        {
          "id": "item-001",
          "name": "菜单项名称",
          "icon": "📱",
          "type": "web|mobile|doc|swagger",
          "url": "http://localhost:3000",
          "description": "描述信息",
          "projectPath": "projects/xxx",
          "viewport": { "width": 375, "height": 812 },
          "docFileId": "doc-xxx.md",
          "docFileName": "文档名称.md",
          "docDescription": "文档描述",
          "route": "/page-path"
        }
      ]
    }
  ]
}
```

### 4.2 数据存储（localStorage）
项目使用 localStorage 进行数据持久化，无需后端服务：

| 存储键 | 说明 |
|-------|------|
| `manager-menu-config` | 菜单配置数据 |
| `manager-user-config` | 用户配置数据（缓存） |
| `manager-uploaded-files` | 上传的 Markdown 文件内容 |
| `token` | 用户登录令牌 |
| `userInfo` | 用户信息（JSON） |

### 4.3 用户配置（public/users.json）
用户账号和权限配置文件：

```json
{
  "title": "ProtoHub 用户配置",
  "version": "1.0.0",
  "users": [
    { "id": 1, "username": "admin", "password": "admin123", "nickname": "管理员", "role": "admin" },
    { "id": 2, "username": "guest", "password": "guest123", "nickname": "访客", "role": "guest" }
  ],
  "permissions": {
    "admin": { "features": ["dashboard", "menu-editor", "projects", "settings"], "canEditMenu": true },
    "guest": { "features": ["dashboard", "projects"], "canEditMenu": false }
  }
}
```

### 4.4 权限说明
| 角色 | 仪表盘 | 菜单编辑器 | 项目列表 | 内容查看 | 系统设置 |
|-----|-------|----------|---------|---------|---------|
| admin | ✅ | ✅ | ✅ | ✅ | ✅ |
| guest | ✅ | ❌ | ✅ | ✅ | ❌ |

## 5. 页面结构

### 5.1 路由配置
| 路径 | 页面 | 权限 |
|-----|------|-----|
| /login | 登录页 | 无需登录 |
| /dashboard | 仪表盘 | 需登录 |
| /menu-editor | 菜单编辑器 | 需登录 + admin |
| /projects | 项目列表 | 需登录 |
| /project/:id | 项目详情 | 需登录 |
| /content/:type/:id | 内容查看 | 需登录 |
| /settings | 系统设置 | 需登录 + admin |

## 6. 状态管理

### 6.1 用户状态（user.js）
- token: 登录令牌
- userInfo: 用户信息
- userConfig: 用户配置（从 users.json 加载）
- isLoggedIn: 登录状态
- canEditMenu: 是否可编辑菜单
- loadUserConfig(): 加载用户配置
- login(): 登录方法
- logout(): 登出方法
- hasPermission(): 权限检查方法

### 6.2 菜单状态（menu.js）
- menuConfig: 菜单配置
- groups: 分组列表
- currentItemId: 当前选中项
- CRUD 操作方法
- 拖拽排序方法
- **持久化方式**：存储到 localStorage，首次加载时从 `public/menus.json` 读取

### 6.3 应用状态（app.js）
- sidebarCollapsed: 侧边栏折叠状态
- theme: 主题
- breadcrumb: 面包屑
- 主题切换方法

## 7. 组件设计

### 7.1 布局组件
- `Layout/index.vue`: 主布局
- `Layout/Sidebar.vue`: 侧边栏
- `Layout/Header.vue`: 顶部导航

### 7.2 业务组件
- `HelpBubble/index.vue`: 帮助气泡
- `MenuEditor/index.vue`: 菜单编辑器主组件
- `MenuEditor/GroupEditor.vue`: 分组编辑器
- `MenuEditor/ItemEditor.vue`: 菜单项编辑器
- `MenuEditor/Preview.vue`: 实时预览
- `DocViewer/index.vue`: 文档查看器
- `IframeViewer/index.vue`: iframe 查看器

## 8. 样式规范

### 8.1 CSS 变量
- 主色调：--primary-color: #007AFF
- 背景色：--bg-primary, --bg-secondary
- 文字色：--text-primary, --text-secondary
- 边框色：--border-color, --border-light
- 圆角：--radius-sm(6px), --radius-md(10px), --radius-lg(16px)

### 8.2 字体
- 字体族：-apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto...
- 字体大小：11px, 13px, 15px, 17px, 20px, 24px, 32px

## 9. 验收标准

1. 项目可正常启动和编译
2. 登录功能正常，各角色账号可正常登录
3. 权限控制正常：
   - admin 可访问所有功能
   - guest 只能访问仪表盘和项目列表，无法看到/访问菜单编辑器和系统设置
4. 侧边栏菜单根据角色正确显示/隐藏
5. 菜单编辑器可正常添加/编辑/删除/拖拽排序
6. 文档查看器可正常渲染 Markdown
7. iframe 查看器可正常加载外部页面
8. 移动端模拟器显示正确
9. 主题切换功能正常
