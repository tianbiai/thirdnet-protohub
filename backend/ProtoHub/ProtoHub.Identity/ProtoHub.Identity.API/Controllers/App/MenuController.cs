using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProtoHub.Identity.API.Map;
using ProtoHub.Identity.Database;
using ProtoHub.Identity.Database.Models;

namespace ProtoHub.Identity.API.Controllers.App;

/// <summary>
/// 菜单查询控制器（应用端）
/// </summary>
[ApiController]
[Route("api/app/menu")]
[Authorize]
public class MenuController : ControllerBase
{
    private readonly ProtoHubDbContext _dbContext;

    public MenuController(ProtoHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 获取菜单配置
    /// </summary>
    /// <returns>菜单配置</returns>
    [HttpPost("list")]
    public async Task<ActionResult<MenuConfigResponse>> List()
    {
        // 从数据库查询分组和菜单项
        var groups = await _dbContext.MenuGroups
            .OrderBy(g => g.order)
            .ToListAsync();

        var items = await _dbContext.MenuItems
            .OrderBy(i => i.order)
            .ToListAsync();

        var response = new MenuConfigResponse
        {
            Groups = groups.Select(g => new MenuGroupResponse
            {
                Id = g.id,
                Name = g.name,
                Icon = g.icon,
                Order = g.order,
                Children = items
                    .Where(i => i.group_id == g.id)
                    .Select(i => new MenuItemResponse
                    {
                        Id = i.id,
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
                    })
                    .ToList()
            }).ToList()
        };

        return Ok(response);
    }
}
