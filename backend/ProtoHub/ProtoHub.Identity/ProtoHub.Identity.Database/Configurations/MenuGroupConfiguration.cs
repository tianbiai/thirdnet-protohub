using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProtoHub.Identity.Database.Models;

namespace ProtoHub.Identity.Database.Configurations;

/// <summary>
/// 菜单分组表配置
/// </summary>
public class MenuGroupConfiguration : IEntityTypeConfiguration<MenuGroupModel>
{
    public void Configure(EntityTypeBuilder<MenuGroupModel> builder)
    {
        builder.ToTable("t_menu_group", t => t.HasComment("菜单分组表"));

        builder.HasKey(x => x.id);

        builder.Property(x => x.id).HasComment("主键ID");

        builder.Property(x => x.name)
            .IsRequired()
            .HasComment("分组名称");

        builder.Property(x => x.icon)
            .HasComment("图标（emoji 或图片 URL）");

        builder.Property(x => x.order)
            .HasDefaultValue(0)
            .HasComment("排序序号");

        builder.Property(x => x.create_time)
            .HasDefaultValueSql("now()")
            .HasComment("创建时间");

        builder.Property(x => x.update_time)
            .HasDefaultValueSql("now()")
            .HasComment("更新时间");

        // 索引
        builder.HasIndex(x => x.order)
            .HasDatabaseName("idx_order");
    }
}
