using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProtoHub.Database.Models;

namespace ProtoHub.Database.Configurations;

/// <summary>
/// 角色权限关联表配置
/// </summary>
public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermissionModel>
{
    public void Configure(EntityTypeBuilder<RolePermissionModel> builder)
    {
        builder.ToTable("t_role_permission", "protohub", t => t.HasComment("角色权限关联表"));

        // 复合主键
        builder.HasKey(x => new { x.role_id, x.permission_id });

        builder.Property(x => x.role_id)
            .HasComment("角色ID");

        builder.Property(x => x.permission_id)
            .HasComment("权限ID");

        builder.Property(x => x.create_time)
            .HasDefaultValueSql("now()")
            .HasComment("创建时间");

        // 索引
        builder.HasIndex(x => x.permission_id)
            .HasDatabaseName("idx_role_permission_permission_id");

        // 种子数据
        var baseTime = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // admin 角色拥有全部权限（显式分配，不进行特殊处理）
        var adminPermissionIds = new long[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 20, 21, 22, 23, 24, 25, 26, 27, 28 };
        var adminSeedData = adminPermissionIds.Select(pid => new RolePermissionModel
        {
            role_id = 1,
            permission_id = pid,
            create_time = baseTime
        }).ToArray();

        builder.HasData(adminSeedData);

        // guest 角色仅有仪表板权限
        builder.HasData(
            new RolePermissionModel { role_id = 2, permission_id = 1, create_time = baseTime }  // dashboard
        );
    }
}
