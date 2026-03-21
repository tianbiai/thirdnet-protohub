using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProtoHub.Identity.Database.Models;

namespace ProtoHub.Identity.Database.Configurations;

/// <summary>
/// 用户表配置
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<UserModel>
{
    public void Configure(EntityTypeBuilder<UserModel> builder)
    {
        builder.ToTable("t_user", t => t.HasComment("用户表"));

        builder.HasKey(x => x.id);

        builder.Property(x => x.id).HasComment("主键ID");

        builder.Property(x => x.user_name)
            .IsRequired()
            .HasComment("用户名");

        builder.Property(x => x.password)
            .IsRequired()
            .HasComment("密码（加密存储）");

        builder.Property(x => x.nickname)
            .IsRequired()
            .HasComment("昵称");

        builder.Property(x => x.email)
            .HasComment("邮箱");

        builder.Property(x => x.status)
            .HasDefaultValue(1)
            .HasComment("状态（0=禁用, 1=启用）");

        builder.Property(x => x.description)
            .HasComment("描述");

        builder.Property(x => x.create_time)
            .HasDefaultValueSql("now()")
            .HasComment("创建时间");

        builder.Property(x => x.update_time)
            .HasDefaultValueSql("now()")
            .HasComment("更新时间");

        // 索引
        builder.HasIndex(x => x.user_name)
            .IsUnique()
            .HasDatabaseName("idx_user_name");

        builder.HasIndex(x => x.email)
            .HasDatabaseName("idx_user_email");

        // 导航属性
        builder.HasMany(x => x.UserRoles)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.user_id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.ProjectAccesses)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.user_id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
