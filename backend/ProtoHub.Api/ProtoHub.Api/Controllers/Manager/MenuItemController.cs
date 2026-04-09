using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProtoHub.Api.DTOs;
using ProtoHub.Api.Extensions;
using ProtoHub.Api.Filters;
using ProtoHub.Database;
using ProtoHub.Database.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ThirdNet.Core.AspNetCore;

namespace ProtoHub.Api.Controllers.Manager
{
    /// <summary>
    /// 菜单项管理控制器（项目子项表）
    /// </summary>
    [ApiController]
    [Route("api/manager/menu/item")]
    public class MenuItemController : ControllerBase
    {
        private readonly ProtoHubDbContext _dbContext;

        public MenuItemController(ProtoHubDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// 获取菜单项列表
        /// </summary>
        [HttpPost("list")]
        [Authorize]
        public async Task<IActionResult> GetList([FromBody] MenuItemListRequest request)
        {
            var query = _dbContext.MenuItems.AsNoTracking();

            if (request.group_id.HasValue)
            {
                query = query.Where(i => i.group_id == request.group_id.Value);
            }

            var items = await query
                .OrderBy(i => i.group_id)
                .ThenBy(i => i.order)
                .Select(i => new MenuItemListResponse
                {
                    id = i.id,
                    group_id = i.group_id,
                    name = i.name,
                    type = i.type,
                    url = i.url,
                    description = i.description,
                    order = i.order,
                    viewport_config = i.viewport_config,
                    doc_file_id = i.doc_file_id,
                    doc_file_name = i.doc_file_name,
                    doc_description = i.doc_description,
                    route = i.route,
                    create_time = i.create_time,
                    update_time = i.update_time
                })
                .ToListAsync();

            return Ok(items);
        }

        /// <summary>
        /// 获取菜单项详情
        /// </summary>
        [HttpPost("{id}/detail")]
        [Authorize]
        public async Task<IActionResult> GetDetail(long id)
        {
            var item = await _dbContext.MenuItems
                .AsNoTracking()
                .Where(i => i.id == id)
                .EnsureExistsAsync("菜单项");

            return Ok(new MenuItemListResponse
            {
                id = item.id,
                group_id = item.group_id,
                name = item.name,
                type = item.type,
                url = item.url,
                description = item.description,
                order = item.order,
                viewport_config = item.viewport_config,
                doc_file_id = item.doc_file_id,
                doc_file_name = item.doc_file_name,
                doc_description = item.doc_description,
                route = item.route,
                create_time = item.create_time,
                update_time = item.update_time
            });
        }

        /// <summary>
        /// 创建菜单项
        /// </summary>
        [HttpPost("create")]
        [Authorize]
        [HasPermission("projects:manage-item")]
        public async Task<IActionResult> Create([FromBody] CreateMenuItemRequest request)
        {
            // 验证分组是否存在
            var groupExists = await _dbContext.MenuGroups.AnyAsync(g => g.id == request.group_id);
            if (!groupExists)
            {
                throw new WebApiException(System.Net.HttpStatusCode.BadRequest, "所属分组不存在");
            }

            var item = new MenuItemModel
            {
                group_id = request.group_id ?? 0,
                name = request.name ?? string.Empty,
                type = request.type ?? "web",
                url = request.url,
                description = request.description,
                order = request.order ?? 0,
                viewport_config = request.viewport_config,
                doc_file_id = request.doc_file_id,
                doc_file_name = request.doc_file_name,
                doc_description = request.doc_description,
                route = request.route
            };

            _dbContext.MenuItems.Add(item);
            await _dbContext.SaveChangesAsync();

            return Ok(new { id = item.id, message = "创建成功" });
        }

        /// <summary>
        /// 更新菜单项
        /// </summary>
        [HttpPost("update")]
        [Authorize]
        [HasPermission("projects:manage-item")]
        public async Task<IActionResult> Update([FromBody] UpdateMenuItemRequest request)
        {
            var item = await _dbContext.MenuItems
                .Where(i => i.id == request.id)
                .EnsureExistsAsync("菜单项");

            if (request.group_id.HasValue) item.group_id = request.group_id.Value;
            if (request.name != null) item.name = request.name;
            if (request.type != null) item.type = request.type;
            if (request.url != null) item.url = request.url;
            if (request.description != null) item.description = request.description;
            if (request.order.HasValue) item.order = request.order.Value;
            if (request.viewport_config != null) item.viewport_config = request.viewport_config;
            if (request.doc_file_id != null) item.doc_file_id = request.doc_file_id;
            if (request.doc_file_name != null) item.doc_file_name = request.doc_file_name;
            if (request.doc_description != null) item.doc_description = request.doc_description;
            if (request.route != null) item.route = request.route;

            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "更新成功" });
        }

        /// <summary>
        /// 删除菜单项
        /// </summary>
        [HttpPost("delete")]
        [Authorize]
        [HasPermission("projects:manage-item")]
        public async Task<IActionResult> Delete([FromBody] DeleteRequest request)
        {
            var item = await _dbContext.MenuItems
                .Where(i => i.id == request.id)
                .EnsureExistsAsync("菜单项");

            _dbContext.MenuItems.Remove(item);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "删除成功" });
        }

        /// <summary>
        /// 重新排序
        /// </summary>
        [HttpPost("reorder")]
        [Authorize]
        [HasPermission("projects:manage-item")]
        public async Task<IActionResult> Reorder([FromBody] ReorderRequest request)
        {
            var items = await _dbContext.MenuItems
                .Where(i => request.ids.Contains(i.id))
                .ToListAsync();

            var itemDict = items.ToDictionary(it => it.id);
            for (int i = 0; i < request.ids.Count; i++)
            {
                if (itemDict.TryGetValue(request.ids[i], out var item))
                {
                    item.order = i + 1;
                }
            }

            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "排序成功" });
        }
    }

}
