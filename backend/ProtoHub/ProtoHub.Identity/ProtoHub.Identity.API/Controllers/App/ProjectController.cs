using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProtoHub.Identity.API.Map;
using ProtoHub.Identity.API.Services;
using ProtoHub.Identity.Database;

namespace ProtoHub.Identity.API.Controllers.App;

/// <summary>
/// 项目控制器（应用端）
/// </summary>
[ApiController]
[Route("api/app/project")]
[Authorize]
public class ProjectController : ControllerBase
{
    private readonly ProtoHubDbContext _dbContext;
    private readonly IPermissionService _permissionService;

    public ProjectController(
        ProtoHubDbContext dbContext,
        IPermissionService permissionService)
    {
        _dbContext = dbContext;
        _permissionService = permissionService;
    }

    /// <summary>
    /// 获取用户可访问的项目列表
    /// </summary>
    [HttpPost("list")]
    public async Task<ActionResult<List<ProjectItem>>> List()
    {
        var userId = GetCurrentUserId();
        var accessibleProjectIds = await _permissionService.GetAccessibleProjectIdsAsync(userId);

        var projects = await _dbContext.MenuGroups
            .Where(g => accessibleProjectIds.Contains(g.id))
            .OrderBy(g => g.order)
            .ToListAsync();

        var items = projects.Select(p => new ProjectItem
        {
            Id = p.id,
            Name = p.name,
            Icon = p.icon,
            Order = p.order
        }).ToList();

        return Ok(items);
    }

    /// <summary>
    /// 获取项目详情
    /// </summary>
    [HttpPost("{id}/detail")]
    public async Task<ActionResult<ProjectDetail>> Detail(long id)
    {
        var userId = GetCurrentUserId();

        // 检查是否有权限访问
        if (!await _permissionService.CanAccessProjectAsync(userId, id))
        {
            return Forbid();
        }

        var project = await _dbContext.MenuGroups
            .FirstOrDefaultAsync(g => g.id == id);

        if (project == null)
        {
            return NotFound(new ErrorResponse
            {
                Code = "RESOURCE_NOT_FOUND",
                Message = "项目不存在"
            });
        }

        // 获取项目下的菜单项
        var menuItems = await _dbContext.MenuItems
            .Where(i => i.group_id == id)
            .OrderBy(i => i.order)
            .ToListAsync();

        return Ok(new ProjectDetail
        {
            Id = project.id,
            Name = project.name,
            Icon = project.icon,
            Order = project.order,
            Items = menuItems.Select(i => new MenuItemInfo
            {
                Id = i.id,
                Name = i.name,
                Type = i.type,
                Path = i.url ?? i.route,
                Permission = i.permission,
                Order = i.order
            }).ToList()
        });
    }

    /// <summary>
    /// 获取当前用户 ID
    /// </summary>
    private long GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null && long.TryParse(userIdClaim.Value, out var userId) ? userId : 0;
    }
}

#region 项目相关 DTO

/// <summary>
/// 项目项 DTO
/// </summary>
public class ProjectItem
{
    /// <summary>
    /// 项目 ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 项目名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 图标
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// 项目详情 DTO
/// </summary>
public class ProjectDetail : ProjectItem
{
    /// <summary>
    /// 菜单项列表
    /// </summary>
    public List<MenuItemInfo> Items { get; set; } = [];
}

/// <summary>
/// 菜单项信息 DTO
/// </summary>
public class MenuItemInfo
{
    /// <summary>
    /// ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 类型
    /// </summary>
    public string Type { get; set; } = null!;

    /// <summary>
    /// 路径
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// 所需权限
    /// </summary>
    public string? Permission { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int Order { get; set; }
}

#endregion
