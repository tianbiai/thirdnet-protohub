using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProtoHub.Database.Models;

namespace ProtoHub.Database.Configurations;

/// <summary>
/// 角色表配置
/// </summary>
public class RoleConfiguration : IEntityTypeConfiguration<RoleModel>
{
    public void Configure(EntityTypeBuilder<RoleModel> builder)
    {
        builder.ToTable("t_role", "protohub", t => t.HasComment("角色表"));

        builder.HasKey(x => x.id);

        builder.Property(x => x.id)
            .HasComment("主键ID");

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

        // 种子数据 - 默认角色
        builder.HasData(
            new RoleModel
            {
                id = 1,
                code = "admin",
                name = "系统管理员",
                description = "拥有系统所有权限",
                is_system = true,
                create_time = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                update_time = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new RoleModel
            {
                id = 2,
                code = "guest",
                name = "访客",
                description = "只读权限",
                is_system = true,
                create_time = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                update_time = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
