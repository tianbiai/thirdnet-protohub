using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProtoHub.Database;
using ProtoHub.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProtoHub.Api.Filters;
using ProtoHub.Api.Extensions;
using ProtoHub.Api.DTOs;
using ThirdNet.Core.AspNetCore;

namespace ProtoHub.Api.Controllers.Manager
{
    /// <summary>
    /// 菜单分组管理控制器（项目表）
    /// </summary>
    [ApiController]
    [Route("api/manager/menu/group")]
    public class MenuGroupController : ControllerBase
    {
        private readonly ProtoHubDbContext _dbContext;

        public MenuGroupController(ProtoHubDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// 获取菜单分组列表
        /// </summary>
        [HttpPost("list")]
        [Authorize]
        public async Task<IActionResult> GetList()
        {
            var groups = await _dbContext.MenuGroups
                .AsNoTracking()
                .OrderBy(g => g.order)
                .Select(g => new MenuGroupListResponse
                {
                    id = g.id,
                    name = g.name,
                    icon = g.icon,
                    order = g.order,
                    description = g.description,
                    create_time = g.create_time,
                    update_time = g.update_time
                })
                .ToListAsync();

            return Ok(groups);
        }

        /// <summary>
        /// 创建菜单分组
        /// </summary>
        [HttpPost("create")]
        [Authorize]
        [HasPermission("projects:create")]
        public async Task<IActionResult> Create([FromBody] CreateMenuGroupRequest request)
        {
            var group = new MenuGroupModel
            {
                name = request.name,
                icon = "protohub",
                order = request.order,
                description = request.description
            };

            _dbContext.MenuGroups.Add(group);
            await _dbContext.SaveChangesAsync();

            // 创建项目成员访问记录
            var currentUserId = User.TryGetUserId();

            if (request.members != null && request.members.Count > 0)
            {
                foreach (var member in request.members)
                {
                    _dbContext.UserProjectAccesses.Add(new UserProjectAccessModel
                    {
                        user_id = member.user_id,
                        project_id = group.id,
                        access_type = member.access_type ?? "view",
                        granted_by = currentUserId ?? member.user_id
                    });
                }
                await _dbContext.SaveChangesAsync();
            }
            else if (currentUserId.HasValue)
            {
                // 无成员时默认将创建者添加为管理员
                _dbContext.UserProjectAccesses.Add(new UserProjectAccessModel
                {
                    user_id = currentUserId.Value,
                    project_id = group.id,
                    access_type = "manage",
                    granted_by = currentUserId.Value
                });
                await _dbContext.SaveChangesAsync();
            }

            return Ok(new { id = group.id, message = "创建成功" });
        }

        /// <summary>
        /// 更新菜单分组
        /// </summary>
        [HttpPost("update")]
        [Authorize]
        [HasPermission("projects:update")]
        public async Task<IActionResult> Update([FromBody] UpdateMenuGroupRequest request)
        {
            var group = await _dbContext.MenuGroups.EnsureExistsAsync(request.id, "分组");

            group.name = request.name ?? group.name;
            group.order = request.order ?? group.order;
            group.description = request.description ?? group.description;

            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "更新成功" });
        }

        /// <summary>
        /// 删除菜单分组
        /// </summary>
        [HttpPost("delete")]
        [Authorize]
        [HasPermission("projects:delete")]
        public async Task<IActionResult> Delete([FromBody] DeleteRequest request)
        {
            var group = await _dbContext.MenuGroups.EnsureExistsAsync(request.id, "分组");

            // 检查是否有关联的菜单项
            var hasItems = await _dbContext.MenuItems.AnyAsync(i => i.group_id == request.id);
            if (hasItems)
            {
                throw new WebApiException(System.Net.HttpStatusCode.BadRequest, "该分组下存在菜单项，请先删除菜单项");
            }

            _dbContext.MenuGroups.Remove(group);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "删除成功" });
        }

        /// <summary>
        /// 重新排序
        /// </summary>
        [HttpPost("reorder")]
        [Authorize]
        [HasPermission("projects:update")]
        public async Task<IActionResult> Reorder([FromBody] ReorderRequest request)
        {
            var groups = await _dbContext.MenuGroups
                .Where(g => request.ids.Contains(g.id))
                .ToListAsync();

            var groupDict = groups.ToDictionary(g => g.id);
            for (int i = 0; i < request.ids.Count; i++)
            {
                if (groupDict.TryGetValue(request.ids[i], out var group))
                {
                    group.order = i + 1;
                }
            }

            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "排序成功" });
        }
    }
}
