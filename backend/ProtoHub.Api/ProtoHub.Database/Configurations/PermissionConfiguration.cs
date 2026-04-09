using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProtoHub.Database.Models;

namespace ProtoHub.Database.Configurations;

/// <summary>
/// 权限表配置
/// </summary>
public class PermissionConfiguration : IEntityTypeConfiguration<PermissionModel>
{
    public void Configure(EntityTypeBuilder<PermissionModel> builder)
    {
        builder.ToTable("t_permission", "protohub", t => t.HasComment("权限表"));

        builder.HasKey(x => x.id);

        builder.Property(x => x.id)
            .HasComment("主键ID");

        builder.Property(x => x.code)
            .IsRequired()
            .HasComment("权限编码");

        builder.Property(x => x.name)
            .IsRequired()
            .HasComment("权限名称");

        builder.Property(x => x.category)
            .HasComment("分类");

        builder.Property(x => x.description)
            .HasComment("描述");

        // 索引
        builder.HasIndex(x => x.code)
            .IsUnique()
            .HasDatabaseName("idx_permission_code");

        builder.HasIndex(x => x.category)
            .HasDatabaseName("idx_permission_category");

        // 种子数据 - 操作级别权限
        builder.HasData(
            // 系统类权限
            new PermissionModel { id = 1, code = "dashboard", name = "仪表板", category = "系统", description = "访问仪表板" },
            new PermissionModel { id = 2, code = "settings", name = "系统设置", category = "系统", description = "系统设置管理" },
            // 用户管理
            new PermissionModel { id = 3, code = "user-manage:view", name = "查看用户", category = "系统", description = "查看用户列表" },
            new PermissionModel { id = 4, code = "user-manage:create", name = "创建用户", category = "系统", description = "创建新用户" },
            new PermissionModel { id = 5, code = "user-manage:update", name = "编辑用户", category = "系统", description = "编辑用户信息" },
            new PermissionModel { id = 6, code = "user-manage:delete", name = "删除用户", category = "系统", description = "删除用户" },
            new PermissionModel { id = 7, code = "user-manage:assign-role", name = "分配用户角色", category = "系统", description = "为用户分配角色" },
            // 角色管理
            new PermissionModel { id = 8, code = "role-manage:view", name = "查看角色", category = "系统", description = "查看角色列表" },
            new PermissionModel { id = 9, code = "role-manage:create", name = "创建角色", category = "系统", description = "创建新角色" },
            new PermissionModel { id = 10, code = "role-manage:update", name = "编辑角色", category = "系统", description = "编辑角色信息" },
            new PermissionModel { id = 11, code = "role-manage:delete", name = "删除角色", category = "系统", description = "删除角色" },
            new PermissionModel { id = 12, code = "role-manage:assign-permission", name = "分配角色权限", category = "系统", description = "为角色分配权限" },
            // 权限管理
            new PermissionModel { id = 13, code = "permission-manage:view", name = "查看权限", category = "系统", description = "查看权限列表" },
            // 系统菜单管理
            new PermissionModel { id = 14, code = "system-menu-manage:view", name = "查看系统菜单", category = "系统", description = "查看系统菜单列表" },
            new PermissionModel { id = 15, code = "system-menu-manage:create", name = "创建系统菜单", category = "系统", description = "创建系统菜单" },
            new PermissionModel { id = 16, code = "system-menu-manage:update", name = "编辑系统菜单", category = "系统", description = "编辑系统菜单" },
            new PermissionModel { id = 17, code = "system-menu-manage:delete", name = "删除系统菜单", category = "系统", description = "删除系统菜单" },

            // 项目类权限
            new PermissionModel { id = 20, code = "projects:view", name = "查看项目列表", category = "项目", description = "查看项目管理列表" },
            new PermissionModel { id = 21, code = "projects:create", name = "创建项目", category = "项目", description = "创建新项目" },
            new PermissionModel { id = 22, code = "projects:update", name = "编辑项目", category = "项目", description = "编辑项目信息" },
            new PermissionModel { id = 23, code = "projects:delete", name = "删除项目", category = "项目", description = "删除项目" },
            new PermissionModel { id = 24, code = "projects:manage-item", name = "管理菜单项", category = "项目", description = "管理项目菜单项" },
            new PermissionModel { id = 28, code = "projects:view-all", name = "查看全部项目", category = "项目", description = "查看所有项目（不论是否是项目成员）" },
            new PermissionModel { id = 25, code = "project-access:view", name = "查看项目成员", category = "项目", description = "查看项目成员列表" },
            new PermissionModel { id = 26, code = "project-access:grant", name = "授权项目访问", category = "项目", description = "授权用户项目访问" },
            new PermissionModel { id = 27, code = "project-access:revoke", name = "撤销项目访问", category = "项目", description = "撤销用户项目访问" }
        );
    }
}
