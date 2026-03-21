using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProtoHub.Identity.Database.Models;

namespace ProtoHub.Identity.Database.Configurations;

/// <summary>
/// 权限表配置
/// </summary>
public class PermissionConfiguration : IEntityTypeConfiguration<PermissionModel>
{
    public void Configure(EntityTypeBuilder<PermissionModel> builder)
    {
        builder.ToTable("t_permission", t => t.HasComment("权限表"));

        builder.HasKey(x => x.id);

        builder.Property(x => x.id).HasComment("主键ID");

        builder.Property(x => x.code)
            .IsRequired()
            .HasComment("权限编码");

        builder.Property(x => x.name)
            .IsRequired()
            .HasComment("权限名称");

        builder.Property(x => x.category)
            .IsRequired()
            .HasDefaultValue("system")
            .HasComment("分类");

        builder.Property(x => x.description)
            .HasComment("描述");

        // 索引
        builder.HasIndex(x => x.code)
            .IsUnique()
            .HasDatabaseName("idx_permission_code");

        // 导航属性
        builder.HasMany(x => x.RolePermissions)
            .WithOne(x => x.Permission)
            .HasForeignKey(x => x.permission_id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
