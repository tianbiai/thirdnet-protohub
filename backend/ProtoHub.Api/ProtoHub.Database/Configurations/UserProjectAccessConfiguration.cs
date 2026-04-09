using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProtoHub.Database.Models;

namespace ProtoHub.Database.Configurations;

/// <summary>
/// 用户项目访问表配置
/// </summary>
public class UserProjectAccessConfiguration : IEntityTypeConfiguration<UserProjectAccessModel>
{
    public void Configure(EntityTypeBuilder<UserProjectAccessModel> builder)
    {
        builder.ToTable("t_user_project_access", "protohub", t => t.HasComment("用户项目访问表"));

        builder.HasKey(x => x.id);

        builder.Property(x => x.id)
            .HasComment("主键ID");

        builder.Property(x => x.user_id)
            .IsRequired()
            .HasComment("用户ID");

        builder.Property(x => x.project_id)
            .IsRequired()
            .HasComment("项目ID（关联t_menu_group.id）");

        builder.Property(x => x.access_type)
            .IsRequired()
            .HasDefaultValue("view")
            .HasComment("访问类型（view=查看, manage=管理）");

        builder.Property(x => x.granted_by)
            .IsRequired()
            .HasComment("授权人ID");

        builder.Property(x => x.create_time)
            .HasDefaultValueSql("now()")
            .HasComment("创建时间");

        // 索引
        builder.HasIndex(x => x.user_id)
            .HasDatabaseName("idx_user_project_access_user_id");

        builder.HasIndex(x => x.project_id)
            .HasDatabaseName("idx_user_project_access_project_id");

        // 唯一约束：同一用户对同一项目只能有一条访问记录
        builder.HasIndex(x => new { x.user_id, x.project_id })
            .IsUnique()
            .HasDatabaseName("idx_user_project_access_unique");
    }
}
