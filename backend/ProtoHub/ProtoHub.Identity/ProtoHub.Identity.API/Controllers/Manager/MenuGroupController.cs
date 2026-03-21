using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProtoHub.Identity.API.Map;
using ProtoHub.Identity.Database;
using ProtoHub.Identity.Database.Models;
using ThirdNet.Core.AspNetCore;

namespace ProtoHub.Identity.API.Controllers.Manager;

/// <summary>
/// 菜单分组管理控制器（管理端）
/// </summary>
[ApiController]
[Route("api/manager/menu/group")]
[Authorize(Roles = "admin")]
public class MenuGroupController : ControllerBase
{
    private readonly ProtoHubDbContext _dbContext;
    private readonly ILogger<MenuGroupController> _logger;

    public MenuGroupController(
        ProtoHubDbContext dbContext,
        ILogger<MenuGroupController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// 获取分组列表
    /// </summary>
    /// <returns>分组列表</returns>
    [HttpPost("list")]
    public async Task<ActionResult<List<GroupListResponse>>> List()
    {
        var groups = await _dbContext.MenuGroups
            .OrderBy(g => g.order)
            .ToListAsync();

        var itemsCount = await _dbContext.MenuItems
            .GroupBy(i => i.group_id)
            .Select(g => new { GroupId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.GroupId, x => x.Count);

        return Ok(groups.Select(g => new GroupListResponse
        {
            Id = g.id,
            Name = g.name,
            Icon = g.icon,
            Order = g.order,
            ItemCount = itemsCount.GetValueOrDefault(g.id, 0)
        }).ToList());
    }

    /// <summary>
    /// 创建分组
    /// </summary>
    /// <param name="request">创建请求</param>
    /// <returns>创建的分组</returns>
    [HttpPost("create")]
    public async Task<ActionResult<MenuGroupResponse>> Create([FromBody] CreateGroupRequest request)
    {
        // 检查名称是否重复
        var exists = await _dbContext.MenuGroups
            .AnyAsync(g => g.name == request.Name);

        if (exists)
        {
            return Conflict(new ErrorResponse
            {
                Code = "DUPLICATE_NAME",
                Message = "分组名称已存在"
            });
        }

        // 获取最大排序号
        var maxOrder = await _dbContext.MenuGroups
            .MaxAsync(g => (int?)g.order) ?? 0;

        var group = new MenuGroupModel
        {
            name = request.Name,
            icon = request.Icon ?? "📁",
            order = maxOrder + 1
        };

        _dbContext.MenuGroups.Add(group);
        await _dbContext.SaveChangesAsync();

        return Ok(new MenuGroupResponse
        {
            Id = group.id,
            Name = group.name,
            Icon = group.icon,
            Order = group.order
        });
    }

    /// <summary>
    /// 更新分组
    /// </summary>
    /// <param name="request">更新请求</param>
    /// <returns>更新后的分组</returns>
    [HttpPost("update")]
    public async Task<ActionResult<MenuGroupResponse>> Update([FromBody] UpdateGroupRequest request)
    {
        var group = await _dbContext.MenuGroups
            .FirstOrDefaultAsync(g => g.id == request.Id);

        if (group == null)
        {
            return NotFound(new ErrorResponse
            {
                Code = "RESOURCE_NOT_FOUND",
                Message = "分组不存在"
            });
        }

        if (request.Name != null)
            group.name = request.Name;
        if (request.Icon != null)
            group.icon = request.Icon;

        await _dbContext.SaveChangesAsync();

        return Ok(new MenuGroupResponse
        {
            Id = group.id,
            Name = group.name,
            Icon = group.icon,
            Order = group.order
        });
    }

    /// <summary>
    /// 删除分组
    /// </summary>
    /// <param name="request">删除请求</param>
    /// <returns>无返回值</returns>
    [HttpPost("delete")]
    public async Task<ActionResult> Delete([FromBody] DeleteRequest request)
    {
        var group = await _dbContext.MenuGroups
            .FirstOrDefaultAsync(g => g.id == request.Id);

        if (group == null)
        {
            return NotFound(new ErrorResponse
            {
                Code = "RESOURCE_NOT_FOUND",
                Message = "分组不存在"
            });
        }

        // 删除分组下的所有菜单项
        var items = await _dbContext.MenuItems
            .Where(i => i.group_id == request.Id)
            .ToListAsync();
        _dbContext.MenuItems.RemoveRange(items);

        // 删除分组
        _dbContext.MenuGroups.Remove(group);
        await _dbContext.SaveChangesAsync();

        return Ok(null);
    }

    /// <summary>
    /// 分组排序
    /// </summary>
    /// <param name="request">排序请求</param>
    /// <returns>无返回值</returns>
    [HttpPost("reorder")]
    public async Task<ActionResult> Reorder([FromBody] ReorderRequest request)
    {
        foreach (var item in request.Orders)
        {
            var group = await _dbContext.MenuGroups
                .FirstOrDefaultAsync(g => g.id == item.Id);

            if (group != null)
            {
                group.order = item.Order;
            }
        }

        await _dbContext.SaveChangesAsync();

        return Ok(null);
    }
}
