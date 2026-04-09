using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProtoHub.Database;
using ProtoHub.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProtoHub.Api.DTOs;
using ProtoHub.Api.Extensions;
using ProtoHub.Api.Filters;
using ThirdNet.Core.AspNetCore;

namespace ProtoHub.Api.Controllers.Manager
{
    /// <summary>
    /// 系统功能菜单管理控制器
    /// </summary>
    [ApiController]
    [Route("api/manager/system-menu")]
    [Authorize]
    public class SystemMenuController : ControllerBase
    {
        private readonly ProtoHubDbContext _dbContext;

        public SystemMenuController(ProtoHubDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// 获取系统菜单列表
        /// </summary>
        [HttpPost("list")]
        [HasPermission("system-menu-manage:view")]
        public async Task<IActionResult> GetList([FromBody] SystemMenuListRequest request)
        {
            var query = _dbContext.SystemMenus.AsNoTracking();

            // 父级筛选
            if (request.parent_id.HasValue)
            {
                query = query.Where(m => m.parent_id == request.parent_id.Value);
            }

            // 状态筛选
            if (request.is_visible.HasValue)
            {
                query = query.Where(m => m.is_visible == request.is_visible.Value);
            }

            // 关键字搜索
            if (!string.IsNullOrEmpty(request.keyword))
            {
                query = query.Where(m => m.name.Contains(request.keyword) ||
                                         m.code.Contains(request.keyword));
            }

            var list = await query
                .OrderBy(m => m.order)
                .ThenBy(m => m.id)
                .Select(m => new SystemMenuListResponse
                {
                    id = m.id,
                    parent_id = m.parent_id,
                    name = m.name,
                    code = m.code,
                    icon = m.icon,
                    path = m.path,
                    order = m.order,
                    is_visible = m.is_visible,
                    permission = m.permission,
                    create_time = m.create_time,
                    update_time = m.update_time
                })
                .ToListAsync();

            return Ok(list);
        }

        /// <summary>
        /// 获取系统菜单树
        /// </summary>
        [HttpPost("tree")]
        [HasPermission("system-menu-manage:view")]
        public async Task<IActionResult> GetTree()
        {
            var allMenus = await _dbContext.SystemMenus
                .AsNoTracking()
                .OrderBy(m => m.order)
                .ThenBy(m => m.id)
                .Select(m => new SystemMenuTreeResponse
                {
                    id = m.id,
                    parent_id = m.parent_id,
                    name = m.name,
                    code = m.code,
                    icon = m.icon,
                    path = m.path,
                    order = m.order,
                    is_visible = m.is_visible,
                    permission = m.permission
                })
                .ToListAsync();

            // 构建树结构
            var tree = BuildTree(allMenus, null);

            return Ok(tree);
        }

        /// <summary>
        /// 构建菜单树
        /// </summary>
        private List<SystemMenuTreeResponse> BuildTree(List<SystemMenuTreeResponse> allMenus, long? parentId)
        {
            var children = allMenus.Where(m => m.parent_id == parentId).ToList();
            foreach (var child in children)
            {
                child.children = BuildTree(allMenus, child.id);
            }
            return children;
        }

        /// <summary>
        /// 创建系统菜单
        /// </summary>
        [HttpPost("create")]
        [HasPermission("system-menu-manage:create")]
        public async Task<IActionResult> Create([FromBody] CreateSystemMenuRequest request)
        {
            // 检查编码是否已存在
            var exists = await _dbContext.SystemMenus.AnyAsync(m => m.code == request.code);
            if (exists)
            {
                throw new WebApiException(System.Net.HttpStatusCode.BadRequest, "菜单编码已存在");
            }

            // 如果有父级，检查父级是否存在
            if (request.parent_id.HasValue)
            {
                var parentExists = await _dbContext.SystemMenus.AnyAsync(m => m.id == request.parent_id.Value);
                if (!parentExists)
                {
                    throw new WebApiException(System.Net.HttpStatusCode.BadRequest, "父级菜单不存在");
                }
            }

            var menu = new SystemMenuModel
            {
                parent_id = request.parent_id,
                name = request.name,
                code = request.code,
                icon = request.icon,
                path = request.path,
                order = request.order,
                is_visible = request.is_visible,
                permission = request.permission
            };

            _dbContext.SystemMenus.Add(menu);
            await _dbContext.SaveChangesAsync();

            return Ok(new { id = menu.id, message = "创建成功" });
        }

        /// <summary>
        /// 更新系统菜单
        /// </summary>
        [HttpPost("update")]
        [HasPermission("system-menu-manage:update")]
        public async Task<IActionResult> Update([FromBody] UpdateSystemMenuRequest request)
        {
            var menu = await _dbContext.SystemMenus.EnsureExistsAsync(request.id, "菜单");

            // 检查编码是否重复
            if (!string.IsNullOrEmpty(request.code) && request.code != menu.code)
            {
                var exists = await _dbContext.SystemMenus.AnyAsync(m => m.code == request.code && m.id != request.id);
                if (exists)
                {
                    throw new WebApiException(System.Net.HttpStatusCode.BadRequest, "菜单编码已存在");
                }
                menu.code = request.code;
            }

            // 检查父级是否有效（不能将自己设为父级，也不能将子级设为父级）
            if (request.parent_id.HasValue)
            {
                if (request.parent_id.Value == request.id)
                {
                    throw new WebApiException(System.Net.HttpStatusCode.BadRequest, "不能将自己设为父级");
                }

                // 检查是否是子孙节点
                var isDescendant = await IsDescendant(request.id, request.parent_id.Value);
                if (isDescendant)
                {
                    throw new WebApiException(System.Net.HttpStatusCode.BadRequest, "不能将子级菜单设为父级");
                }
            }

            if (request.parent_id.HasValue) menu.parent_id = request.parent_id.Value == 0 ? null : request.parent_id;
            if (request.name != null) menu.name = request.name;
            if (request.icon != null) menu.icon = request.icon;
            if (request.path != null) menu.path = request.path;
            if (request.order.HasValue) menu.order = request.order.Value;
            if (request.is_visible.HasValue) menu.is_visible = request.is_visible.Value;
            if (request.permission != null) menu.permission = request.permission;

            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "更新成功" });
        }

        /// <summary>
        /// 检查是否是子孙节点
        /// </summary>
        private async Task<bool> IsDescendant(long ancestorId, long descendantId)
        {
            var descendant = await _dbContext.SystemMenus.FirstOrDefaultAsync(m => m.id == descendantId);
            if (descendant == null || descendant.parent_id == null)
            {
                return false;
            }

            if (descendant.parent_id == ancestorId)
            {
                return true;
            }

            return await IsDescendant(ancestorId, descendant.parent_id.Value);
        }

        /// <summary>
        /// 删除系统菜单
        /// </summary>
        [HttpPost("delete")]
        [HasPermission("system-menu-manage:delete")]
        public async Task<IActionResult> Delete([FromBody] DeleteRequest request)
        {
            var menu = await _dbContext.SystemMenus.EnsureExistsAsync(request.id, "菜单");

            // 检查是否有子菜单
            var hasChildren = await _dbContext.SystemMenus.AnyAsync(m => m.parent_id == request.id);
            if (hasChildren)
            {
                throw new WebApiException(System.Net.HttpStatusCode.BadRequest, "存在子菜单，请先删除子菜单");
            }

            // 删除角色菜单关联
            var roleMenus = await _dbContext.RoleSystemMenus.Where(rm => rm.system_menu_id == request.id).ToListAsync();
            _dbContext.RoleSystemMenus.RemoveRange(roleMenus);

            // 删除菜单
            _dbContext.SystemMenus.Remove(menu);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "删除成功" });
        }

        /// <summary>
        /// 获取角色可见的功能菜单
        /// </summary>
        [HttpPost("role/{id}/menus")]
        [HasPermission("system-menu-manage:view")]
        public async Task<IActionResult> GetRoleMenus(long id)
        {
            var menus = await (from rsm in _dbContext.RoleSystemMenus
                               join m in _dbContext.SystemMenus on rsm.system_menu_id equals m.id
                               where rsm.role_id == id
                               orderby m.order
                               select new SystemMenuSimpleResponse
                               {
                                   id = m.id,
                                   name = m.name,
                                   code = m.code,
                                   icon = m.icon,
                                   path = m.path
                               }).ToListAsync();

            return Ok(menus);
        }

        /// <summary>
        /// 分配角色功能菜单
        /// </summary>
        [HttpPost("role/{id}/assign-menus")]
        [HasPermission("system-menu-manage:assign")]
        public async Task<IActionResult> AssignRoleMenus(long id, [FromBody] AssignMenusRequest request)
        {
            // 检查角色是否存在
            var role = await _dbContext.Roles.AnyAsync(r => r.id == id);
            if (!role)
            {
                throw new WebApiException(System.Net.HttpStatusCode.NotFound, "角色不存在");
            }

            // 删除现有菜单关联
            var existingMenus = await _dbContext.RoleSystemMenus.Where(rsm => rsm.role_id == id).ToListAsync();
            _dbContext.RoleSystemMenus.RemoveRange(existingMenus);

            // 添加新菜单关联
            foreach (var menuId in request.menu_ids)
            {
                _dbContext.RoleSystemMenus.Add(new RoleSystemMenuModel
                {
                    role_id = id,
                    system_menu_id = menuId
                });
            }

            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "分配成功" });
        }

        /// <summary>
        /// 重新排序
        /// </summary>
        [HttpPost("reorder")]
        [HasPermission("system-menu-manage:update")]
        public async Task<IActionResult> Reorder([FromBody] ReorderRequest request)
        {
            var menus = await _dbContext.SystemMenus
                .Where(m => request.ids.Contains(m.id))
                .ToListAsync();

            var menuDict = menus.ToDictionary(m => m.id);
            for (int i = 0; i < request.ids.Count; i++)
            {
                if (menuDict.TryGetValue(request.ids[i], out var menu))
                {
                    menu.order = i + 1;
                }
            }

            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "排序成功" });
        }
    }

}
