using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProtoHub.Database.Models;

namespace ProtoHub.Database.Configurations;

/// <summary>
/// 系统功能菜单表配置
/// </summary>
public class SystemMenuConfiguration : IEntityTypeConfiguration<SystemMenuModel>
{
    public void Configure(EntityTypeBuilder<SystemMenuModel> builder)
    {
        builder.ToTable("t_system_menu", "protohub", t => t.HasComment("系统功能菜单表"));

        builder.HasKey(x => x.id);

        builder.Property(x => x.id)
            .HasComment("主键ID");

        builder.Property(x => x.parent_id)
            .HasComment("父菜单ID（支持多级）");

        builder.Property(x => x.name)
            .IsRequired()
            .HasComment("菜单名称");

        builder.Property(x => x.code)
            .IsRequired()
            .HasComment("菜单编码");

        builder.Property(x => x.icon)
            .HasComment("图标");

        builder.Property(x => x.path)
            .HasComment("路由路径");

        builder.Property(x => x.order)
            .HasDefaultValue(0)
            .HasComment("排序");

        builder.Property(x => x.is_visible)
            .HasDefaultValue(true)
            .HasComment("是否可见");

        builder.Property(x => x.permission)
            .HasComment("所需权限");

        builder.Property(x => x.create_time)
            .HasDefaultValueSql("now()")
            .HasComment("创建时间");

        builder.Property(x => x.update_time)
            .HasDefaultValueSql("now()")
            .HasComment("更新时间");

        // 索引
        builder.HasIndex(x => x.code)
            .IsUnique()
            .HasDatabaseName("idx_system_menu_code");

        builder.HasIndex(x => x.parent_id)
            .HasDatabaseName("idx_system_menu_parent_id");

        // 种子数据 - 默认系统菜单
        builder.HasData(
            // 系统管理（父菜单）
            new SystemMenuModel
            {
                id = 1,
                parent_id = null,
                name = "系统管理",
                code = "system",
                icon = "setting",
                path = "/system",
                order = 100,
                is_visible = true,
                permission = null,
                create_time = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                update_time = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            // 用户管理
            new SystemMenuModel
            {
                id = 2,
                parent_id = 1,
                name = "用户管理",
                code = "user-manage",
                icon = "user",
                path = "/system/user",
                order = 1,
                is_visible = true,
                permission = "user-manage:view",
                create_time = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                update_time = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            // 角色管理
            new SystemMenuModel
            {
                id = 3,
                parent_id = 1,
                name = "角色管理",
                code = "role-manage",
                icon = "team",
                path = "/system/role",
                order = 2,
                is_visible = true,
                permission = "role-manage:view",
                create_time = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                update_time = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            // 权限管理
            new SystemMenuModel
            {
                id = 4,
                parent_id = 1,
                name = "权限管理",
                code = "permission-manage",
                icon = "lock",
                path = "/system/permission",
                order = 3,
                is_visible = true,
                permission = "permission-manage:view",
                create_time = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                update_time = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            // 系统菜单
            new SystemMenuModel
            {
                id = 5,
                parent_id = 1,
                name = "系统菜单",
                code = "system-menu-manage",
                icon = "menu",
                path = "/system/menu",
                order = 4,
                is_visible = true,
                permission = "system-menu-manage:view",
                create_time = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                update_time = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            // 项目管理（父菜单）
            new SystemMenuModel
            {
                id = 10,
                parent_id = null,
                name = "项目管理",
                code = "project",
                icon = "project",
                path = "/project",
                order = 200,
                is_visible = true,
                permission = null,
                create_time = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                update_time = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            // 项目列表
            new SystemMenuModel
            {
                id = 11,
                parent_id = 10,
                name = "项目列表",
                code = "project-list",
                icon = "appstore",
                path = "/project/list",
                order = 1,
                is_visible = true,
                permission = "projects:view",
                create_time = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                update_time = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            // 项目成员
            new SystemMenuModel
            {
                id = 12,
                parent_id = 10,
                name = "项目成员",
                code = "project-access",
                icon = "team",
                path = "/project/access",
                order = 2,
                is_visible = true,
                permission = "project-access:view",
                create_time = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                update_time = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
