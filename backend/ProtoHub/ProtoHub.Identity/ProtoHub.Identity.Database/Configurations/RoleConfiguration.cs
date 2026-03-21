using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProtoHub.Identity.Database.Models;

namespace ProtoHub.Identity.Database.Configurations;

/// <summary>
/// 角色表配置
/// </summary>
public class RoleConfiguration : IEntityTypeConfiguration<RoleModel>
{
    public void Configure(EntityTypeBuilder<RoleModel> builder)
    {
        builder.ToTable("t_role", t => t.HasComment("角色表"));

        builder.HasKey(x => x.id);

        builder.Property(x => x.id).HasComment("主键ID");

        builder.Property(x => x.code)
            .IsRequired()
            .HasComment("角色编码");

        builder.Property(x => x.name)
            .IsRequired()
            .HasComment("角色名称");

        builder.Property(x => x.description)
            .HasComment("描述");

        builder.Property(x => x.is_system)
            .HasDefaultValue(false)
            .HasComment("是否系统内置");

        builder.Property(x => x.create_time)
            .HasDefaultValueSql("now()")
            .HasComment("创建时间");

        builder.Property(x => x.update_time)
            .HasDefaultValueSql("now()")
            .HasComment("更新时间");

        // 索引
        builder.HasIndex(x => x.code)
            .IsUnique()
            .HasDatabaseName("idx_role_code");

        // 导航属性
        builder.HasMany(x => x.RolePermissions)
            .WithOne(x => x.Role)
            .HasForeignKey(x => x.role_id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.UserRoles)
            .WithOne(x => x.Role)
            .HasForeignKey(x => x.role_id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
