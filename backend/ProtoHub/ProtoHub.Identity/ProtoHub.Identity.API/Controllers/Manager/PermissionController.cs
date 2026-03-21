using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProtoHub.Identity.API.Map;
using ProtoHub.Identity.Database;

namespace ProtoHub.Identity.API.Controllers.Manager;

/// <summary>
/// 权限管理控制器（管理端）
/// </summary>
[ApiController]
[Route("api/manager/permission")]
[Authorize]
public class PermissionController : ControllerBase
{
    private readonly ProtoHubDbContext _dbContext;

    public PermissionController(ProtoHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 获取权限列表
    /// </summary>
    [HttpPost("list")]
    public async Task<ActionResult<List<PermissionItem>>> List([FromBody] PermissionListRequest? request)
    {
        var query = _dbContext.Permissions.AsQueryable();

        // 分类筛选
        if (request != null && !string.IsNullOrEmpty(request.Category))
        {
            query = query.Where(p => p.category == request.Category);
        }

        var permissions = await query
            .OrderBy(p => p.category)
            .ThenBy(p => p.id)
            .ToListAsync();

        var items = permissions.Select(p => new PermissionItem
        {
            Id = p.id,
            Code = p.code,
            Name = p.name,
            Category = p.category,
            Description = p.description
        }).ToList();

        return Ok(items);
    }

    /// <summary>
    /// 获取权限分类列表
    /// </summary>
    [HttpPost("categories")]
    public async Task<ActionResult<List<string>>> Categories()
    {
        var categories = await _dbContext.Permissions
            .Select(p => p.category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        return Ok(categories);
    }
}
