using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProtoHub.Identity.Database.Models;

namespace ProtoHub.Identity.Database.Configurations;

/// <summary>
/// 用户角色关联表配置
/// </summary>
public class UserRoleConfiguration : IEntityTypeConfiguration<UserRoleModel>
{
    public void Configure(EntityTypeBuilder<UserRoleModel> builder)
    {
        builder.ToTable("t_user_role", t => t.HasComment("用户角色关联表"));

        builder.HasKey(x => x.id);

        builder.Property(x => x.id).HasComment("主键ID");

        builder.Property(x => x.user_id).HasComment("用户ID");

        builder.Property(x => x.role_id).HasComment("角色ID");

        // 唯一索引：一个用户对一个角色只有一条记录
        builder.HasIndex(x => new { x.user_id, x.role_id })
            .IsUnique()
            .HasDatabaseName("idx_user_role_unique");

        // 导航属性
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.user_id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Role)
            .WithMany(x => x.UserRoles)
            .HasForeignKey(x => x.role_id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
