using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProtoHub.Database.Models;

namespace ProtoHub.Database.Configurations;

/// <summary>
/// 菜单项表配置（项目子项表）
/// </summary>
public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItemModel>
{
    public void Configure(EntityTypeBuilder<MenuItemModel> builder)
    {
        builder.ToTable("t_menu_item", "protohub", t => t.HasComment("菜单项表（项目子项表）"));

        builder.HasKey(x => x.id);

        builder.Property(x => x.id)
            .HasComment("主键ID");

        builder.Property(x => x.group_id)
            .IsRequired()
            .HasComment("所属分组ID");

        builder.Property(x => x.name)
            .IsRequired()
            .HasComment("菜单名称");

        builder.Property(x => x.type)
            .IsRequired()
            .HasDefaultValue("web")
            .HasComment("类型（web/miniprogram/doc/link/internal）");

        builder.Property(x => x.url)
            .HasComment("链接地址");

        builder.Property(x => x.description)
            .HasComment("描述");

        builder.Property(x => x.order)
            .HasDefaultValue(0)
            .HasComment("排序");

        // JSONB 类型配置 - 视口配置（使用 ValueConverter 避免 OwnsOne + ToJson 的 snapshot 不稳定问题）
        builder.Property(x => x.viewport_config)
            .HasColumnType("jsonb")
            .HasComment("视口配置")
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => v == null ? null : JsonSerializer.Deserialize<ViewportConfigModel>(v, (JsonSerializerOptions?)null));

        builder.Property(x => x.doc_file_id)
            .HasComment("文档文件ID");

        builder.Property(x => x.doc_file_name)
            .HasComment("文档文件名");

        builder.Property(x => x.doc_description)
            .HasComment("文档描述");

        builder.Property(x => x.route)
            .HasComment("内部路由");

        builder.Property(x => x.create_time)
            .HasDefaultValueSql("now()")
            .HasComment("创建时间");

        builder.Property(x => x.update_time)
            .HasDefaultValueSql("now()")
            .HasComment("更新时间");

        // 索引
        builder.HasIndex(x => x.group_id)
            .HasDatabaseName("idx_menu_item_group_id");

        builder.HasIndex(x => new { x.group_id, x.order })
            .HasDatabaseName("idx_menu_item_group_order");

        // 注意：由于使用了 JSONB 类型（ToJson），不支持 HasData 种子数据
        // 种子数据将在应用启动时通过代码插入
    }
}
