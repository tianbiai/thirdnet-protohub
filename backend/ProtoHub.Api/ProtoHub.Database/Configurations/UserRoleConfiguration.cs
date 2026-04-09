using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProtoHub.Database.Models;

namespace ProtoHub.Database.Configurations;

/// <summary>
/// 用户角色关联表配置
/// </summary>
public class UserRoleConfiguration : IEntityTypeConfiguration<UserRoleModel>
{
    public void Configure(EntityTypeBuilder<UserRoleModel> builder)
    {
        builder.ToTable("t_user_role", "protohub", t => t.HasComment("用户角色关联表"));

        // 复合主键
        builder.HasKey(x => new { x.user_id, x.role_id });

        builder.Property(x => x.user_id)
            .HasComment("用户ID");

        builder.Property(x => x.role_id)
            .HasComment("角色ID");

        builder.Property(x => x.create_time)
            .HasDefaultValueSql("now()")
            .HasComment("创建时间");

        // 索引
        builder.HasIndex(x => x.role_id)
            .HasDatabaseName("idx_user_role_role_id");

        // 种子数据 - admin 用户分配管理员角色
        builder.HasData(new UserRoleModel
        {
            user_id = 1,
            role_id = 1,
            create_time = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
