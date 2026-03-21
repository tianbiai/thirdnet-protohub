# ProtoHub 后端项目规划

## 项目概述

ProtoHub（原型视界）后端服务，基于 ThirdNet.Core.IdentityService 模板创建的单体认证服务。

## 技术栈

- .NET 10
- ThirdNet.Core.AspNetCore 10.0.3
- PostgreSQL (Npgsql.EntityFrameworkCore.PostgreSQL 10.0.1)
- JWT (RSA) 认证

## 项目结构

```
backend/ProtoHub/
├── Tools/
│   ├── ProtoHub.Common/         # 通用工具类库
│   └── ProtoHub.Cache/          # 缓存工具类库
└── ProtoHub.Identity/           # 认证服务
    ├── ProtoHub.Identity.slnx   # 解决方案文件
    ├── ProtoHub.Identity.API/   # Web API 主项目
    │   ├── Controllers/
    │   │   ├── App/              # 应用端控制器
    │   │   │   ├── UserController.cs
    │   │   │   └── MenuController.cs
    │   │   └── Manager/         # 管理端控制器
    │   │       ├── MenuGroupController.cs
    │   │       └── MenuItemController.cs
    │   ├── Map/                 # DTO 映射
    │   │   ├── UserMap.cs
    │   │   └── MenuMap.cs
    │   ├── Startup.cs
    │   └── appsettings.json
    └── ProtoHub.Identity.Database/  # 数据库层
        ├── Models/                # 实体模型
        │   ├── User.cs
        │   ├── MenuGroup.cs
        │   ├── MenuItem.cs
        │   └── ViewportConfig.cs
        ├── Configurations/       # Fluent API 配置
        │   ├── UserConfiguration.cs
        │   ├── MenuGroupConfiguration.cs
        │   └── MenuItemConfiguration.cs
        └── ProtoHubDbContext.cs  # DbContext
```

## API 接口

### 应用端 (App)

| 路由 | 说明 | 权限 |
|-----|------|------|
| POST /api/app/user/login | 用户登录 | 无 |
| POST /api/app/user/logout | 用户登出 | 已认证 |
| POST /api/app/user/current | 获取当前用户 | 已认证 |
| POST /api/app/menu/list | 获取菜单配置 | 已认证 |

### 管理端 (Manager)

| 路由 | 说明 | 权限 |
|-----|------|------|
| POST /api/manager/menu/group/list | 分组列表 | admin |
| POST /api/manager/menu/group/create | 创建分组 | admin |
| POST /api/manager/menu/group/update | 更新分组 | admin |
| POST /api/manager/menu/group/delete | 删除分组 | admin |
| POST /api/manager/menu/group/reorder | 分组排序 | admin |
| POST /api/manager/menu/item/list | 菜单项列表 | admin |
| POST /api/manager/menu/item/create | 创建菜单项 | admin |
| POST /api/manager/menu/item/update | 更新菜单项 | admin |
| POST /api/manager/menu/item/delete | 删除菜单项 | admin |
| POST /api/manager/menu/item/reorder | 菜单项排序 | admin |

## 数据模型

- **User**: 用户（Id, Username, Password, Nickname, Role, Description）
- **MenuGroup**: 菜单分组（Id, Name, Icon, Order）
- **MenuItem**: 菜单项（Id, GroupId, Name, Icon, Type, Url, Description, Viewport, ...）

## 配置说明

修改 `appsettings.json` 中的 `DefaultConnectionString` 配置数据库连接。

## 运行命令

```bash
cd backend/ProtoHub/ProtoHub.Identity
dotnet run --project ProtoHub.Identity.API
```

服务将运行在 http://localhost:6003

## 下一步

1. 创建数据库迁移
2. 添加种子数据（默认用户）
3. 前端对接 API
