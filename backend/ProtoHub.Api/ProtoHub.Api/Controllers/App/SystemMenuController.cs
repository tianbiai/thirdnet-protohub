using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProtoHub.Api.DTOs;
using ProtoHub.Database;
using ProtoHub.Database.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ThirdNet.Core.AspNetCore;

namespace ProtoHub.Api.Controllers.App
{
    /// <summary>
    /// 系统功能菜单控制器（应用端）
    /// </summary>
    [ApiController]
    [Route("api/app/system-menu")]
    [Authorize]
    public class SystemMenuController : ControllerBase
    {
        private readonly ProtoHubDbContext _dbContext;

        public SystemMenuController(ProtoHubDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// 获取当前用户可见的功能菜单（子菜单驱动：父菜单仅在有可见子菜单时显示）
        /// </summary>
        [HttpPost("my-menus")]
        public async Task<IActionResult> GetMyMenus()
        {
            // 查询所有可见菜单
            var allMenus = await _dbContext.SystemMenus
                .AsNoTracking()
                .Where(m => m.is_visible)
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

            // 从 token 获取用户权限列表
            var userPermissions = User.FindAll("permission").Select(c => c.Value).ToList();

            // 按权限过滤菜单
            var filteredMenus = allMenus.Where(m =>
                string.IsNullOrEmpty(m.permission) ||
                userPermissions.Contains(m.permission)
            ).ToList();

            // 构建树结构（子菜单驱动：父菜单仅在有可见子菜单时显示）
            var tree = BuildTree(filteredMenus, null);

            // 剪枝：移除没有子菜单的父节点（permission 为空的顶级菜单如果没有可见子菜单则隐藏）
            tree = tree.Where(t => t.children.Count > 0 || !string.IsNullOrEmpty(t.permission)).ToList();

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
                // 子菜单驱动剪枝：如果父菜单没有可见子菜单，且自身没有权限要求，则不添加子节点
            }
            return children;
        }
    }
}
