using Microsoft.EntityFrameworkCore;
using ProtoHub.Identity.Database;

namespace ProtoHub.Identity.API.Services;

/// <summary>
/// 权限服务接口
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// 获取用户的所有权限编码
    /// </summary>
    Task<HashSet<string>> GetUserPermissionCodesAsync(long userId);

    /// <summary>
    /// 检查用户是否拥有指定权限
    /// </summary>
    Task<bool> HasPermissionAsync(long userId, string permissionCode);

    /// <summary>
    /// 检查用户是否是管理员
    /// </summary>
    Task<bool> IsAdminAsync(long userId);

    /// <summary>
    /// 获取用户可访问的项目ID列表
    /// </summary>
    Task<HashSet<long>> GetAccessibleProjectIdsAsync(long userId);

    /// <summary>
    /// 获取用户可管理的项目ID列表
    /// </summary>
    Task<HashSet<long>> GetManageableProjectIdsAsync(long userId);

    /// <summary>
    /// 检查用户是否可以访问指定项目
    /// </summary>
    Task<bool> CanAccessProjectAsync(long userId, long projectId);

    /// <summary>
    /// 检查用户是否可以管理指定项目
    /// </summary>
    Task<bool> CanManageProjectAsync(long userId, long projectId);
}

/// <summary>
/// 权限服务实现
/// </summary>
public class PermissionService : IPermissionService
{
    private readonly ProtoHubDbContext _dbContext;

    public PermissionService(ProtoHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 获取用户的所有权限编码
    /// </summary>
    public async Task<HashSet<string>> GetUserPermissionCodesAsync(long userId)
    {
        // 检查是否是管理员
        if (await IsAdminAsync(userId))
        {
            // 管理员隐式拥有所有权限
            return (await _dbContext.Permissions.ToListAsync())
                .Select(p => p.code)
                .ToHashSet();
        }

        // 获取用户角色关联的权限
        var permissions = await (
            from ur in _dbContext.UserRoles
            join rp in _dbContext.RolePermissions on ur.role_id equals rp.role_id
            join p in _dbContext.Permissions on rp.permission_id equals p.id
            where ur.user_id == userId
            select p.code
        ).Distinct().ToListAsync();

        return permissions.ToHashSet();
    }

    /// <summary>
    /// 检查用户是否拥有指定权限
    /// </summary>
    public async Task<bool> HasPermissionAsync(long userId, string permissionCode)
    {
        // 管理员拥有所有权限
        if (await IsAdminAsync(userId))
        {
            return true;
        }

        var permissions = await GetUserPermissionCodesAsync(userId);
        return permissions.Contains(permissionCode);
    }

    /// <summary>
    /// 检查用户是否是管理员
    /// </summary>
    public async Task<bool> IsAdminAsync(long userId)
    {
        var userRoles = await _dbContext.UserRoles
            .Include(ur => ur.Role)
            .Where(ur => ur.user_id == userId)
            .Select(ur => ur.Role.code)
            .ToListAsync();

        return userRoles.Contains("admin");
    }

    /// <summary>
    /// 获取用户可访问的项目ID列表
    /// </summary>
    public async Task<HashSet<long>> GetAccessibleProjectIdsAsync(long userId)
    {
        // 管理员可访问所有项目
        if (await IsAdminAsync(userId))
        {
            var allProjectIds = await _dbContext.MenuGroups
                .Select(g => g.id)
                .ToListAsync();
            return allProjectIds.ToHashSet();
        }

        // 获取用户被授权的项目
        var projectIds = await _dbContext.UserProjectAccesses
            .Where(a => a.user_id == userId)
            .Select(a => a.project_id)
            .ToListAsync();

        return projectIds.ToHashSet();
    }

    /// <summary>
    /// 获取用户可管理的项目ID列表
    /// </summary>
    public async Task<HashSet<long>> GetManageableProjectIdsAsync(long userId)
    {
        // 管理员可管理所有项目
        if (await IsAdminAsync(userId))
        {
            var allProjectIds = await _dbContext.MenuGroups
                .Select(g => g.id)
                .ToListAsync();
            return allProjectIds.ToHashSet();
        }

        // 获取用户有管理权限的项目
        var projectIds = await _dbContext.UserProjectAccesses
            .Where(a => a.user_id == userId && a.access_type == "manage")
            .Select(a => a.project_id)
            .ToListAsync();

        return projectIds.ToHashSet();
    }

    /// <summary>
    /// 检查用户是否可以访问指定项目
    /// </summary>
    public async Task<bool> CanAccessProjectAsync(long userId, long projectId)
    {
        var accessibleProjects = await GetAccessibleProjectIdsAsync(userId);
        return accessibleProjects.Contains(projectId);
    }

    /// <summary>
    /// 检查用户是否可以管理指定项目
    /// </summary>
    public async Task<bool> CanManageProjectAsync(long userId, long projectId)
    {
        var manageableProjects = await GetManageableProjectIdsAsync(userId);
        return manageableProjects.Contains(projectId);
    }
}
