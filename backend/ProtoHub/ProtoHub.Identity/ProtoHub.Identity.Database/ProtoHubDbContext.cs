using Microsoft.EntityFrameworkCore;
using ProtoHub.Identity.Database.Configurations;
using ProtoHub.Identity.Database.Models;

namespace ProtoHub.Identity.Database;

/// <summary>
/// ProtoHub 数据库上下文
/// </summary>
public class ProtoHubDbContext : DbContext
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public ProtoHubDbContext(DbContextOptions<ProtoHubDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// 用户表
    /// </summary>
    public DbSet<UserModel> Users => Set<UserModel>();

    /// <summary>
    /// 角色表
    /// </summary>
    public DbSet<RoleModel> Roles => Set<RoleModel>();

    /// <summary>
    /// 权限表
    /// </summary>
    public DbSet<PermissionModel> Permissions => Set<PermissionModel>();

    /// <summary>
    /// 用户角色关联表
    /// </summary>
    public DbSet<UserRoleModel> UserRoles => Set<UserRoleModel>();

    /// <summary>
    /// 角色权限关联表
    /// </summary>
    public DbSet<RolePermissionModel> RolePermissions => Set<RolePermissionModel>();

    /// <summary>
    /// 用户项目访问表
    /// </summary>
    public DbSet<UserProjectAccessModel> UserProjectAccesses => Set<UserProjectAccessModel>();

    /// <summary>
    /// 菜单分组表（项目表）
    /// </summary>
    public DbSet<MenuGroupModel> MenuGroups => Set<MenuGroupModel>();

    /// <summary>
    /// 菜单项表
    /// </summary>
    public DbSet<MenuItemModel> MenuItems => Set<MenuItemModel>();

    /// <summary>
    /// 配置实体映射
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
        modelBuilder.ApplyConfiguration(new UserProjectAccessConfiguration());
        modelBuilder.ApplyConfiguration(new MenuGroupConfiguration());
        modelBuilder.ApplyConfiguration(new MenuItemConfiguration());

        // 添加种子数据
        SeedData(modelBuilder);
    }

    /// <summary>
    /// 添加种子数据
    /// </summary>
    private static void SeedData(ModelBuilder modelBuilder)
    {
        // 种子权限
        var permissions = new[]
        {
            // 系统管理类
            new PermissionModel { id = 1, code = "user:create", name = "创建用户", category = "system", description = "创建新用户账号" },
            new PermissionModel { id = 2, code = "user:read", name = "查看用户", category = "system", description = "查看用户列表和详情" },
            new PermissionModel { id = 3, code = "user:update", name = "编辑用户", category = "system", description = "编辑用户信息" },
            new PermissionModel { id = 4, code = "user:delete", name = "删除用户", category = "system", description = "删除用户账号" },
            new PermissionModel { id = 5, code = "role:read", name = "查看角色", category = "system", description = "查看角色列表和详情" },
            new PermissionModel { id = 6, code = "role:manage", name = "管理角色", category = "system", description = "创建、编辑、删除角色" },
            // 项目管理类
            new PermissionModel { id = 10, code = "project:create", name = "创建项目", category = "project", description = "创建新项目" },
            new PermissionModel { id = 11, code = "project:read", name = "查看项目", category = "project", description = "查看项目列表和详情" },
            new PermissionModel { id = 12, code = "project:update", name = "编辑项目", category = "project", description = "编辑项目信息" },
            new PermissionModel { id = 13, code = "project:delete", name = "删除项目", category = "project", description = "删除项目" },
            new PermissionModel { id = 14, code = "project:grant", name = "授权项目访问", category = "project", description = "为用户授权项目访问权限" }
        };
        modelBuilder.Entity<PermissionModel>().HasData(permissions);

        // 种子角色
        var roles = new[]
        {
            new RoleModel { id = 1, code = "admin", name = "系统管理员", description = "拥有所有权限", is_system = true },
            new RoleModel { id = 2, code = "project_manager", name = "项目经理", description = "可管理分配给自己的项目，可为用户授权访问这些项目", is_system = true },
            new RoleModel { id = 3, code = "user", name = "普通用户", description = "只能访问被授权的项目", is_system = true }
        };
        modelBuilder.Entity<RoleModel>().HasData(roles);

        // 角色权限关联（admin 不需要显式关联，代码中隐式拥有所有权限）
        var rolePermissions = new List<RolePermissionModel>
        {
            // project_manager 权限
            new() { id = 1, role_id = 2, permission_id = 11 }, // project:read
            new() { id = 2, role_id = 2, permission_id = 12 }, // project:update
            new() { id = 3, role_id = 2, permission_id = 14 }, // project:grant
            // user 权限
            new() { id = 4, role_id = 3, permission_id = 11 }  // project:read
        };
        modelBuilder.Entity<RolePermissionModel>().HasData(rolePermissions);

        // 种子管理员用户（密码：admin123）
        modelBuilder.Entity<UserModel>().HasData(new UserModel
        {
            id = 1,
            user_name = "admin",
            password = "admin123",
            nickname = "系统管理员",
            email = "admin@protohub.local",
            status = 1,
            description = "系统默认管理员账号",
            create_time = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            update_time = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        // 管理员用户角色关联
        modelBuilder.Entity<UserRoleModel>().HasData(new UserRoleModel
        {
            id = 1,
            user_id = 1,
            role_id = 1
        });
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
    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 更新实体的时间戳
    /// </summary>
    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added ||
                        e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            var now = DateTime.UtcNow;

            if (entry.State == EntityState.Added)
            {
                if (entry.Entity is UserModel user)
                    user.create_time = now;
                else if (entry.Entity is RoleModel role)
                    role.create_time = now;
                else if (entry.Entity is UserProjectAccessModel access)
                    access.create_time = now;
                else if (entry.Entity is MenuGroupModel group)
                    group.create_time = now;
                else if (entry.Entity is MenuItemModel item)
                    item.create_time = now;
            }

            if (entry.Entity is UserModel u)
                u.update_time = now;
            else if (entry.Entity is RoleModel r)
                r.update_time = now;
            else if (entry.Entity is MenuGroupModel g)
                g.update_time = now;
            else if (entry.Entity is MenuItemModel i)
                i.update_time = now;
        }
    }
}
