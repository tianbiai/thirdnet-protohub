using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProtoHub.Database.Models;

namespace ProtoHub.Database.Configurations;

/// <summary>
/// 用户表配置
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<UserModel>
{
    public void Configure(EntityTypeBuilder<UserModel> builder)
    {
        builder.ToTable("t_user", "protohub", t => t.HasComment("用户表"));

        builder.HasKey(x => x.id);

        builder.Property(x => x.id)
            .HasComment("主键ID");

        builder.Property(x => x.user_name)
            .IsRequired()
            .HasComment("用户名");

        builder.Property(x => x.password)
            .IsRequired()
            .HasComment("密码");

        builder.Property(x => x.nick_name)
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

        // 种子数据 - 默认管理员账户（密码: admin123，BCrypt 哈希）
        builder.HasData(new UserModel
        {
            id = 1,
            user_name = "admin",
            password = "$2a$12$7ag2WeNZaLTotEEw/kyDF.4Qw1rPXXDtvYtzZlenMN0i49L5gKSAu",
            nick_name = "管理员",
            email = "admin@protohub.com",
            status = 1,
            description = "系统默认管理员",
            create_time = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            update_time = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
