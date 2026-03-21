using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProtoHub.Identity.Database.Models;

namespace ProtoHub.Identity.Database.Configurations;

/// <summary>
/// 菜单项表配置
/// </summary>
public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItemModel>
{
    public void Configure(EntityTypeBuilder<MenuItemModel> builder)
    {
        builder.ToTable("t_menu_item", t => t.HasComment("菜单项表"));

        builder.HasKey(x => x.id);

        builder.Property(x => x.id).HasComment("主键ID");

        builder.Property(x => x.group_id)
            .IsRequired()
            .HasComment("所属分组ID");

        builder.Property(x => x.name)
            .IsRequired()
            .HasComment("菜单项名称");

        builder.Property(x => x.icon)
            .HasComment("图标（emoji 或图片 URL）");

        builder.Property(x => x.type)
            .IsRequired()
            .HasComment("类型（web/miniprogram/doc/swagger/internal）");

        builder.Property(x => x.url)
            .HasComment("外部链接地址");

        builder.Property(x => x.description)
            .HasComment("描述信息");

        builder.Property(x => x.order)
            .HasDefaultValue(0)
            .HasComment("排序序号");

        // PostgreSQL jsonb 类型存储视口配置
        builder.Property(x => x.viewport)
            .HasColumnType("jsonb")
            .HasComment("视口配置");

        builder.Property(x => x.doc_file_id)
            .HasComment("文档文件ID");

        builder.Property(x => x.doc_file_name)
            .HasComment("文档文件名");

        builder.Property(x => x.doc_description)
            .HasComment("文档描述");

        builder.Property(x => x.route)
            .HasComment("内部路由路径");

        builder.Property(x => x.permission)
            .HasComment("访问所需权限标识");

        builder.Property(x => x.create_time)
            .HasDefaultValueSql("now()")
            .HasComment("创建时间");

        builder.Property(x => x.update_time)
            .HasDefaultValueSql("now()")
            .HasComment("更新时间");

        // 索引
        builder.HasIndex(x => x.group_id)
            .HasDatabaseName("idx_group_id");

        builder.HasIndex(x => x.order)
            .HasDatabaseName("idx_order");
    }
}
