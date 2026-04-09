using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProtoHub.Api.DTOs;
using ProtoHub.Api.Extensions;
using ProtoHub.Api.Filters;
using ProtoHub.Database;
using System.Linq;
using System.Threading.Tasks;

namespace ProtoHub.Api.Controllers.Manager
{
    /// <summary>
    /// 权限管理控制器
    /// </summary>
    [ApiController]
    [Route("api/manager/permission")]
    [Authorize]
    [HasPermission("permission-manage:view")]
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
        public async Task<IActionResult> GetList([FromBody] PermissionListRequest request)
        {
            var query = _dbContext.Permissions.AsNoTracking();

            // 分类筛选
            if (!string.IsNullOrEmpty(request.category))
            {
                query = query.Where(p => p.category == request.category);
            }

            // 关键字搜索
            if (!string.IsNullOrEmpty(request.keyword))
            {
                query = query.Where(p => p.name.Contains(request.keyword) ||
                                         p.code.Contains(request.keyword));
            }

            // 查询并分页
            var result = await query
                .OrderBy(p => p.category)
                .ThenBy(p => p.id)
                .Select(p => new PermissionListResponse
                {
                    id = p.id,
                    code = p.code,
                    name = p.name,
                    category = p.category,
                    description = p.description
                })
                .ToPagedResultAsync(request.page, request.page_size);

            return Ok(result);
        }

        /// <summary>
        /// 获取所有权限（按分类分组）
        /// </summary>
        [HttpPost("all")]
        public async Task<IActionResult> GetAll()
        {
            var permissions = await _dbContext.Permissions
                .AsNoTracking()
                .OrderBy(p => p.category)
                .ThenBy(p => p.id)
                .Select(p => new PermissionSimpleResponse
                {
                    id = p.id,
                    code = p.code,
                    name = p.name,
                    category = p.category
                })
                .ToListAsync();

            // 按分类分组
            var grouped = permissions
                .GroupBy(p => p.category ?? "未分类")
                .Select(g => new PermissionGroupResponse
                {
                    category = g.Key,
                    permissions = g.ToList()
                })
                .ToList();

            return Ok(grouped);
        }

        /// <summary>
        /// 获取权限分类列表
        /// </summary>
        [HttpPost("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _dbContext.Permissions
                .AsNoTracking()
                .Where(p => p.category != null)
                .Select(p => p.category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return Ok(categories);
        }
    }
}
