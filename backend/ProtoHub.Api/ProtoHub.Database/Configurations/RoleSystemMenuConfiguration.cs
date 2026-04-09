using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProtoHub.Database.Models;

namespace ProtoHub.Database.Configurations;

/// <summary>
/// 角色系统菜单关联表配置
/// </summary>
public class RoleSystemMenuConfiguration : IEntityTypeConfiguration<RoleSystemMenuModel>
{
    public void Configure(EntityTypeBuilder<RoleSystemMenuModel> builder)
    {
        builder.ToTable("t_role_system_menu", "protohub", t => t.HasComment("角色系统菜单关联表"));

        // 复合主键
        builder.HasKey(x => new { x.role_id, x.system_menu_id });

        builder.Property(x => x.role_id)
            .HasComment("角色ID");

        builder.Property(x => x.system_menu_id)
            .HasComment("系统菜单ID");

        builder.Property(x => x.create_time)
            .HasDefaultValueSql("now()")
            .HasComment("创建时间");

        // 索引
        builder.HasIndex(x => x.system_menu_id)
            .HasDatabaseName("idx_role_system_menu_menu_id");

        // 种子数据 - admin 角色拥有所有菜单
        builder.HasData(
            new RoleSystemMenuModel { role_id = 1, system_menu_id = 1, create_time = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RoleSystemMenuModel { role_id = 1, system_menu_id = 2, create_time = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RoleSystemMenuModel { role_id = 1, system_menu_id = 3, create_time = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RoleSystemMenuModel { role_id = 1, system_menu_id = 4, create_time = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RoleSystemMenuModel { role_id = 1, system_menu_id = 5, create_time = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RoleSystemMenuModel { role_id = 1, system_menu_id = 10, create_time = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RoleSystemMenuModel { role_id = 1, system_menu_id = 11, create_time = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RoleSystemMenuModel { role_id = 1, system_menu_id = 12, create_time = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
