using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProtoHub.Api.Extensions;
using ProtoHub.Database;
using ProtoHub.Database.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThirdNet.Core.AspNetCore;

namespace ProtoHub.Api.Controllers.App
{
    /// <summary>
    /// 菜单控制器（应用端）
    /// </summary>
    [ApiController]
    [Route("api/app/menu")]
    public class MenuController : ControllerBase
    {
        private readonly ProtoHubDbContext _dbContext;

        public MenuController(ProtoHubDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// 获取用户可访问的菜单配置
        /// </summary>
        [HttpPost("list")]
        [Authorize]
        public async Task<IActionResult> GetMenuList()
        {
            var userId = User.GetUserId();

            // 检查用户是否有"查看全部项目"权限
            var hasViewAllPermission = User.HasClaim("permission", "projects:view-all");

            List<long> accessibleProjectIds;
            if (hasViewAllPermission)
            {
                // 有查看全部权限，获取所有项目ID
                accessibleProjectIds = await _dbContext.MenuGroups
                    .AsNoTracking()
                    .Select(g => g.id)
                    .ToListAsync();
            }
            else
            {
                // 无查看全部权限，仅获取用户作为成员的项目
                accessibleProjectIds = await _dbContext.UserProjectAccesses
                    .AsNoTracking()
                    .Where(upa => upa.user_id == userId)
                    .Select(upa => upa.project_id)
                    .ToListAsync();
            }

            // 查询项目（菜单分组）
            var groups = await _dbContext.MenuGroups
                .AsNoTracking()
                .Where(g => accessibleProjectIds.Contains(g.id))
                .OrderBy(g => g.order)
                .ToListAsync();

            // 查询项目子项（菜单项）
            var items = await _dbContext.MenuItems
                .AsNoTracking()
                .Where(i => accessibleProjectIds.Contains(i.group_id))
                .OrderBy(i => i.group_id)
                .ThenBy(i => i.order)
                .ToListAsync();

            // 构建响应
            var response = groups.Select(g => new MenuGroupResponse
            {
                id = g.id,
                name = g.name,
                icon = g.icon,
                order = g.order,
                description = g.description,
                items = items
                    .Where(i => i.group_id == g.id)
                    .Select(i => new MenuItemResponse
                    {
                        id = i.id,
                        name = i.name,
                        type = i.type,
                        url = i.url,
                        description = i.description,
                        order = i.order,
                        viewport_config = i.viewport_config,
                        doc_file_id = i.doc_file_id,
                        doc_file_name = i.doc_file_name,
                        doc_description = i.doc_description,
                        route = i.route
                    }).ToList()
            }).ToList();

            return Ok(response);
        }

        /// <summary>
        /// 获取项目详情（含子项）
        /// </summary>
        [HttpPost("{id}/detail")]
        [Authorize]
        public async Task<IActionResult> GetProjectDetail(long id)
        {
            var userId = User.GetUserId();

            // 检查用户是否有权限访问该项目（有 projects:view-all 权限则跳过成员检查）
            var hasViewAllPermission = User.HasClaim("permission", "projects:view-all");

            if (!hasViewAllPermission)
            {
                var hasAccess = await _dbContext.UserProjectAccesses
                    .AsNoTracking()
                    .AnyAsync(upa => upa.user_id == userId && upa.project_id == id);

                if (!hasAccess)
                {
                    throw new WebApiException(System.Net.HttpStatusCode.Forbidden, "无权访问该项目");
                }
            }

            // 查询项目
            var group = await _dbContext.MenuGroups
                .AsNoTracking()
                .Where(g => g.id == id)
                .EnsureExistsAsync("项目");

            // 查询项目子项
            var items = await _dbContext.MenuItems
                .AsNoTracking()
                .Where(i => i.group_id == id)
                .OrderBy(i => i.order)
                .ToListAsync();

            return Ok(new MenuGroupDetailResponse
            {
                id = group.id,
                name = group.name,
                icon = group.icon,
                order = group.order,
                description = group.description,
                items = items.Select(i => new MenuItemResponse
                {
                    id = i.id,
                    name = i.name,
                    type = i.type,
                    url = i.url,
                    description = i.description,
                    order = i.order,
                    viewport_config = i.viewport_config,
                    doc_file_id = i.doc_file_id,
                    doc_file_name = i.doc_file_name,
                    doc_description = i.doc_description,
                    route = i.route
                }).ToList()
            });
        }
    }

    #region DTO 类

    /// <summary>
    /// 菜单分组响应
    /// </summary>
    public class MenuGroupResponse
    {
        public long id { get; set; }
        public string name { get; set; } = string.Empty;
        public string? icon { get; set; }
        public int order { get; set; }
        public string? description { get; set; }
        public List<MenuItemResponse> items { get; set; } = new();
    }

    /// <summary>
    /// 菜单分组详情响应
    /// </summary>
    public class MenuGroupDetailResponse : MenuGroupResponse
    {
    }

    /// <summary>
    /// 菜单项响应
    /// </summary>
    public class MenuItemResponse
    {
        public long id { get; set; }
        public string name { get; set; } = string.Empty;
        public string type { get; set; } = "web";
        public string? url { get; set; }
        public string? description { get; set; }
        public int order { get; set; }
        public ViewportConfigModel? viewport_config { get; set; }
        public string? doc_file_id { get; set; }
        public string? doc_file_name { get; set; }
        public string? doc_description { get; set; }
        public string? route { get; set; }
    }

    #endregion
}
