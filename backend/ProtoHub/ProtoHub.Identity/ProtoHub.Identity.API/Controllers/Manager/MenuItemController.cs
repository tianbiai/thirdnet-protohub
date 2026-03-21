using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProtoHub.Identity.API.Map;
using ProtoHub.Identity.Database;
using ProtoHub.Identity.Database.Models;
using ThirdNet.Core.AspNetCore;

namespace ProtoHub.Identity.API.Controllers.Manager;

/// <summary>
/// 菜单项管理控制器（管理端）
/// </summary>
[ApiController]
[Route("api/manager/menu/item")]
[Authorize(Roles = "admin")]
public class MenuItemController : ControllerBase
{
    private readonly ProtoHubDbContext _dbContext;
    private readonly ILogger<MenuItemController> _logger;

    public MenuItemController(
        ProtoHubDbContext dbContext,
        ILogger<MenuItemController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// 获取分组下的菜单项列表
    /// </summary>
    /// <param name="request">列表请求</param>
    /// <returns>菜单项列表</returns>
    [HttpPost("list")]
    public async Task<ActionResult<List<MenuItemDetailResponse>>> List([FromBody] ItemListRequest request)
    {
        var items = await _dbContext.MenuItems
            .Where(i => i.group_id == request.GroupId)
            .OrderBy(i => i.order)
            .ToListAsync();

        return Ok(items.Select(i => new MenuItemDetailResponse
        {
            Id = i.id,
            GroupId = i.group_id,
            Name = i.name,
            Icon = i.icon,
            Type = i.type,
            Url = i.url,
            Description = i.description,
            Order = i.order,
            Viewport = i.viewport,
            DocFileId = i.doc_file_id,
            DocFileName = i.doc_file_name,
            DocDescription = i.doc_description,
            Route = i.route,
            Permission = i.permission
        }).ToList());
    }

    /// <summary>
    /// 创建菜单项
    /// </summary>
    /// <param name="request">创建请求</param>
    /// <returns>创建的菜单项</returns>
    [HttpPost("create")]
    public async Task<ActionResult<MenuItemDetailResponse>> Create([FromBody] CreateItemRequest request)
    {
        // 验证分组是否存在
        var group = await _dbContext.MenuGroups
            .FirstOrDefaultAsync(g => g.id == request.GroupId);

        if (group == null)
        {
            return NotFound(new ErrorResponse
            {
                Code = "RESOURCE_NOT_FOUND",
                Message = "分组不存在"
            });
        }

        // 验证必填字段
        var validTypes = new[] { "web", "miniprogram", "doc", "swagger", "internal" };
        if (!validTypes.Contains(request.Type))
        {
            return BadRequest(new ErrorResponse
            {
                Code = "VALIDATION_ERROR",
                Message = "type 必须是 web/miniprogram/doc/swagger/internal 之一"
            });
        }

        if ((request.Type == "web" || request.Type == "miniprogram" || request.Type == "swagger")
            && string.IsNullOrEmpty(request.Url))
        {
            return BadRequest(new ErrorResponse
            {
                Code = "VALIDATION_ERROR",
                Message = "type 为 web/miniprogram/swagger 时 url 不能为空"
            });
        }

        // 获取最大排序号
        var maxOrder = await _dbContext.MenuItems
            .Where(i => i.group_id == request.GroupId)
            .MaxAsync(i => (int?)i.order) ?? 0;

        var item = new MenuItemModel
        {
            group_id = request.GroupId,
            name = request.Name,
            icon = request.Icon,
            type = request.Type,
            url = request.Url,
            description = request.Description,
            order = maxOrder + 1,
            viewport = request.Viewport,
            doc_file_id = request.DocFileId,
            doc_file_name = request.DocFileName,
            doc_description = request.DocDescription,
            route = request.Route,
            permission = request.Permission
        };

        _dbContext.MenuItems.Add(item);
        await _dbContext.SaveChangesAsync();

        return Ok(new MenuItemDetailResponse
        {
            Id = item.id,
            GroupId = item.group_id,
            Name = item.name,
            Icon = item.icon,
            Type = item.type,
            Url = item.url,
            Description = item.description,
            Order = item.order,
            Viewport = item.viewport,
            DocFileId = item.doc_file_id,
            DocFileName = item.doc_file_name,
            DocDescription = item.doc_description,
            Route = item.route,
            Permission = item.permission
        });
    }

    /// <summary>
    /// 更新菜单项
    /// </summary>
    /// <param name="request">更新请求</param>
    /// <returns>更新后的菜单项</returns>
    [HttpPost("update")]
    public async Task<ActionResult<MenuItemDetailResponse>> Update([FromBody] UpdateItemRequest request)
    {
        var item = await _dbContext.MenuItems
            .FirstOrDefaultAsync(i => i.id == request.Id);

        if (item == null)
        {
            return NotFound(new ErrorResponse
            {
                Code = "RESOURCE_NOT_FOUND",
                Message = "菜单项不存在"
            });
        }

        // 更新字段
        if (request.Name != null)
            item.name = request.Name;
        if (request.Icon != null)
            item.icon = request.Icon;
        if (request.Type != null)
            item.type = request.Type;
        if (request.Url != null)
            item.url = request.Url;
        if (request.Description != null)
            item.description = request.Description;
        if (request.Viewport != null)
            item.viewport = request.Viewport;
        if (request.DocFileId != null)
            item.doc_file_id = request.DocFileId;
        if (request.DocFileName != null)
            item.doc_file_name = request.DocFileName;
        if (request.DocDescription != null)
            item.doc_description = request.DocDescription;
        if (request.Route != null)
            item.route = request.Route;
        if (request.Permission != null)
            item.permission = request.Permission;

        await _dbContext.SaveChangesAsync();

        return Ok(new MenuItemDetailResponse
        {
            Id = item.id,
            GroupId = item.group_id,
            Name = item.name,
            Icon = item.icon,
            Type = item.type,
            Url = item.url,
            Description = item.description,
            Order = item.order,
            Viewport = item.viewport,
            DocFileId = item.doc_file_id,
            DocFileName = item.doc_file_name,
            DocDescription = item.doc_description,
            Route = item.route,
            Permission = item.permission
        });
    }

    /// <summary>
    /// 删除菜单项
    /// </summary>
    /// <param name="request">删除请求</param>
    /// <returns>无返回值</returns>
    [HttpPost("delete")]
    public async Task<ActionResult> Delete([FromBody] DeleteRequest request)
    {
        var item = await _dbContext.MenuItems
            .FirstOrDefaultAsync(i => i.id == request.Id);

        if (item == null)
        {
            return NotFound(new ErrorResponse
            {
                Code = "RESOURCE_NOT_FOUND",
                Message = "菜单项不存在"
            });
        }

        _dbContext.MenuItems.Remove(item);
        await _dbContext.SaveChangesAsync();

        return Ok(null);
    }

    /// <summary>
    /// 菜单项排序
    /// </summary>
    /// <param name="request">排序请求</param>
    /// <returns>无返回值</returns>
    [HttpPost("reorder")]
    public async Task<ActionResult> Reorder([FromBody] ReorderItemsRequest request)
    {
        foreach (var orderItem in request.Orders)
        {
            var item = await _dbContext.MenuItems
                .FirstOrDefaultAsync(i => i.id == orderItem.Id && i.group_id == request.GroupId);

            if (item != null)
            {
                item.order = orderItem.Order;
            }
        }

        await _dbContext.SaveChangesAsync();

        return Ok(null);
    }
}
