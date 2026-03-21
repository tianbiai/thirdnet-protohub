using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProtoHub.Identity.Database.Models;

namespace ProtoHub.Identity.Database.Configurations;

/// <summary>
/// 角色权限关联表配置
/// </summary>
public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermissionModel>
{
    public void Configure(EntityTypeBuilder<RolePermissionModel> builder)
    {
        builder.ToTable("t_role_permission", t => t.HasComment("角色权限关联表"));

        builder.HasKey(x => x.id);

        builder.Property(x => x.id).HasComment("主键ID");

        builder.Property(x => x.role_id).HasComment("角色ID");

        builder.Property(x => x.permission_id).HasComment("权限ID");

        // 唯一索引：一个角色对一个权限只有一条记录
        builder.HasIndex(x => new { x.role_id, x.permission_id })
            .IsUnique()
            .HasDatabaseName("idx_role_permission_unique");

        // 导航属性
        builder.HasOne(x => x.Role)
            .WithMany(x => x.RolePermissions)
            .HasForeignKey(x => x.role_id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Permission)
            .WithMany(x => x.RolePermissions)
            .HasForeignKey(x => x.permission_id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
