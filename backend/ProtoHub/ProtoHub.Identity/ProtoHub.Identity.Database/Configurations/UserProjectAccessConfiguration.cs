using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProtoHub.Identity.Database.Models;

namespace ProtoHub.Identity.Database.Configurations;

/// <summary>
/// 用户项目访问表配置
/// </summary>
public class UserProjectAccessConfiguration : IEntityTypeConfiguration<UserProjectAccessModel>
{
    public void Configure(EntityTypeBuilder<UserProjectAccessModel> builder)
    {
        builder.ToTable("t_user_project_access", t => t.HasComment("用户项目访问表"));

        builder.HasKey(x => x.id);

        builder.Property(x => x.id).HasComment("主键ID");

        builder.Property(x => x.user_id).HasComment("用户ID");

        builder.Property(x => x.project_id).HasComment("项目ID");

        builder.Property(x => x.access_type)
            .IsRequired()
            .HasDefaultValue("view")
            .HasComment("访问类型（view/manage）");

        builder.Property(x => x.granted_by).HasComment("授权人ID");

        builder.Property(x => x.create_time)
            .HasDefaultValueSql("now()")
            .HasComment("授权时间");

        // 唯一索引：一个用户对一个项目只有一条访问记录
        builder.HasIndex(x => new { x.user_id, x.project_id })
            .IsUnique()
            .HasDatabaseName("idx_user_project_unique");

        // 导航属性
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.user_id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Project)
            .WithMany()
            .HasForeignKey(x => x.project_id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
