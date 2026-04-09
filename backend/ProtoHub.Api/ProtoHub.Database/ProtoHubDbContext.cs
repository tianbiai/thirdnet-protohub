using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ProtoHub.Database.Models;
using ProtoHub.Database.Configurations;
using ProtoHub.Database.Interfaces;

namespace ProtoHub.Database;

/// <summary>
/// ProtoHub 数据库上下文
/// </summary>
public class ProtoHubDbContext : DbContext
{
    public ProtoHubDbContext(DbContextOptions<ProtoHubDbContext> options) : base(options) { }

    /// <summary>
    /// 用户表
    /// </summary>
    public DbSet<UserModel> Users { get; set; } = null!;

    /// <summary>
    /// 角色表
    /// </summary>
    public DbSet<RoleModel> Roles { get; set; } = null!;

    /// <summary>
    /// 权限表
    /// </summary>
    public DbSet<PermissionModel> Permissions { get; set; } = null!;

    /// <summary>
    /// 用户角色关联表
    /// </summary>
    public DbSet<UserRoleModel> UserRoles { get; set; } = null!;

    /// <summary>
    /// 角色权限关联表
    /// </summary>
    public DbSet<RolePermissionModel> RolePermissions { get; set; } = null!;

    /// <summary>
    /// 菜单分组表（项目表）
    /// </summary>
    public DbSet<MenuGroupModel> MenuGroups { get; set; } = null!;

    /// <summary>
    /// 菜单项表（项目子项表）
    /// </summary>
    public DbSet<MenuItemModel> MenuItems { get; set; } = null!;

    /// <summary>
    /// 系统功能菜单表
    /// </summary>
    public DbSet<SystemMenuModel> SystemMenus { get; set; } = null!;

    /// <summary>
    /// 角色系统菜单关联表
    /// </summary>
    public DbSet<RoleSystemMenuModel> RoleSystemMenus { get; set; } = null!;

    /// <summary>
    /// 用户项目访问表
    /// </summary>
    public DbSet<UserProjectAccessModel> UserProjectAccesses { get; set; } = null!;

    /// <summary>
    /// 配置实体
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 应用所有配置
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new PermissionConfiguration());
        modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
        modelBuilder.ApplyConfiguration(new RolePermissionConfiguration());
        modelBuilder.ApplyConfiguration(new MenuGroupConfiguration());
        modelBuilder.ApplyConfiguration(new MenuItemConfiguration());
        modelBuilder.ApplyConfiguration(new SystemMenuConfiguration());
        modelBuilder.ApplyConfiguration(new RoleSystemMenuConfiguration());
        modelBuilder.ApplyConfiguration(new UserProjectAccessConfiguration());
    }

    /// <summary>
    /// 保存更改前自动更新时间戳
    /// </summary>
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    /// <summary>
    /// 异步保存更改前自动更新时间戳
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 更新时间戳
    /// </summary>
    private void UpdateTimestamps()
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is IHasTimestamps timestamped)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        timestamped.create_time = now;
                        timestamped.update_time = now;
                        break;
                    case EntityState.Modified:
                        timestamped.update_time = now;
                        break;
                }
            }
        }
    }
}
