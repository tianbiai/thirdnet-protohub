using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProtoHub.Database;
using ProtoHub.Database.Models;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ProtoHub.Api.DTOs;
using ProtoHub.Api.Extensions;
using ProtoHub.Api.Filters;
using ThirdNet.Core.AspNetCore;

namespace ProtoHub.Api.Controllers.Manager
{
    /// <summary>
    /// 角色管理控制器
    /// </summary>
    [ApiController]
    [Route("api/manager/role")]
    [Authorize]
    public class RoleController : ControllerBase
    {
        private readonly ProtoHubDbContext _dbContext;

        public RoleController(ProtoHubDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// 获取角色列表
        /// </summary>
        [HttpPost("list")]
        [HasPermission("role-manage:view")]
        public async Task<IActionResult> GetList([FromBody] RoleListRequest request)
        {
            var query = _dbContext.Roles.AsNoTracking();

            // 关键字搜索
            if (!string.IsNullOrEmpty(request.keyword))
            {
                query = query.Where(r => r.name.Contains(request.keyword) ||
                                         r.code.Contains(request.keyword));
            }

            var result = await query
                .OrderBy(r => r.id)
                .Select(r => new RoleListResponse
                {
                    id = r.id,
                    code = r.code,
                    name = r.name,
                    description = r.description,
                    is_system = r.is_system,
                    create_time = r.create_time,
                    update_time = r.update_time
                })
                .ToPagedResultAsync(request.page, request.page_size);

            return Ok(result);
        }

        /// <summary>
        /// 获取所有角色（下拉选择用）
        /// </summary>
        [HttpPost("all")]
        [HasPermission("role-manage:view")]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _dbContext.Roles
                .AsNoTracking()
                .OrderBy(r => r.id)
                .Select(r => new RoleSimpleResponse
                {
                    id = r.id,
                    code = r.code,
                    name = r.name
                })
                .ToListAsync();

            return Ok(roles);
        }

        /// <summary>
        /// 获取角色详情
        /// </summary>
        [HttpPost("{id}/detail")]
        [HasPermission("role-manage:view")]
        public async Task<IActionResult> GetDetail(long id)
        {
            var role = await _dbContext.Roles.AsNoTracking().Where(r => r.id == id).EnsureExistsAsync("角色");

            // 获取角色的权限
            var permissions = await (from rp in _dbContext.RolePermissions
                                     join p in _dbContext.Permissions on rp.permission_id equals p.id
                                     where rp.role_id == id
                                     select new PermissionSimpleResponse
                                     {
                                         id = p.id,
                                         code = p.code,
                                         name = p.name,
                                         category = p.category
                                     }).ToListAsync();

            return Ok(new RoleDetailResponse
            {
                id = role.id,
                code = role.code,
                name = role.name,
                description = role.description,
                is_system = role.is_system,
                permissions = permissions,
                create_time = role.create_time,
                update_time = role.update_time
            });
        }

        /// <summary>
        /// 创建角色
        /// </summary>
        [HttpPost("create")]
        [HasPermission("role-manage:create")]
        public async Task<IActionResult> Create([FromBody] RoleCreateRequest request)
        {
            // 检查编码是否已存在
            var exists = await _dbContext.Roles.AnyAsync(r => r.code == request.code);
            if (exists)
            {
                throw new WebApiException(System.Net.HttpStatusCode.BadRequest, "角色编码已存在");
            }

            var role = new RoleModel
            {
                code = request.code,
                name = request.name,
                description = request.description,
                is_system = false
            };

            _dbContext.Roles.Add(role);
            await _dbContext.SaveChangesAsync();

            return Ok(new { id = role.id, message = "创建成功" });
        }

        /// <summary>
        /// 更新角色
        /// </summary>
        [HttpPost("update")]
        [HasPermission("role-manage:update")]
        public async Task<IActionResult> Update([FromBody] RoleUpdateRequest request)
        {
            var role = await _dbContext.Roles.EnsureExistsAsync(request.id, "角色");

            if (role.is_system)
            {
                throw new WebApiException(System.Net.HttpStatusCode.BadRequest, "系统内置角色不可修改");
            }

            // 检查编码是否重复
            if (!string.IsNullOrEmpty(request.code) && request.code != role.code)
            {
                var exists = await _dbContext.Roles.AnyAsync(r => r.code == request.code && r.id != request.id);
                if (exists)
                {
                    throw new WebApiException(System.Net.HttpStatusCode.BadRequest, "角色编码已存在");
                }
                role.code = request.code;
            }

            if (request.name != null) role.name = request.name;
            if (request.description != null) role.description = request.description;

            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "更新成功" });
        }

        /// <summary>
        /// 删除角色
        /// </summary>
        [HttpPost("delete")]
        [HasPermission("role-manage:delete")]
        public async Task<IActionResult> Delete([FromBody] DeleteRequest request)
        {
            var role = await _dbContext.Roles.EnsureExistsAsync(request.id, "角色");

            if (role.is_system)
            {
                throw new WebApiException(System.Net.HttpStatusCode.BadRequest, "系统内置角色不可删除");
            }

            // 删除角色权限关联
            var rolePermissions = await _dbContext.RolePermissions.Where(rp => rp.role_id == request.id).ToListAsync();
            _dbContext.RolePermissions.RemoveRange(rolePermissions);

            // 删除用户角色关联
            var userRoles = await _dbContext.UserRoles.Where(ur => ur.role_id == request.id).ToListAsync();
            _dbContext.UserRoles.RemoveRange(userRoles);

            // 删除角色系统菜单关联
            var roleMenus = await _dbContext.RoleSystemMenus.Where(rm => rm.role_id == request.id).ToListAsync();
            _dbContext.RoleSystemMenus.RemoveRange(roleMenus);

            // 删除角色
            _dbContext.Roles.Remove(role);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "删除成功" });
        }

        /// <summary>
        /// 获取角色权限
        /// </summary>
        [HttpPost("{id}/permissions")]
        [HasPermission("role-manage:view")]
        public async Task<IActionResult> GetRolePermissions(long id)
        {
            var permissions = await (from rp in _dbContext.RolePermissions
                                     join p in _dbContext.Permissions on rp.permission_id equals p.id
                                     where rp.role_id == id
                                     select new PermissionSimpleResponse
                                     {
                                         id = p.id,
                                         code = p.code,
                                         name = p.name,
                                         category = p.category
                                     }).ToListAsync();

            return Ok(permissions);
        }

        /// <summary>
        /// 分配角色权限
        /// </summary>
        [HttpPost("{id}/assign-permissions")]
        [HasPermission("role-manage:assign-permission")]
        public async Task<IActionResult> AssignPermissions(long id, [FromBody] AssignPermissionsRequest request)
        {
            // 检查角色是否存在
            var role = await _dbContext.Roles.EnsureExistsAsync(id, "角色");

            // 删除现有权限
            var existingPermissions = await _dbContext.RolePermissions.Where(rp => rp.role_id == id).ToListAsync();
            _dbContext.RolePermissions.RemoveRange(existingPermissions);

            // 添加新权限
            foreach (var permissionId in request.permission_ids)
            {
                _dbContext.RolePermissions.Add(new RolePermissionModel
                {
                    role_id = id,
                    permission_id = permissionId
                });
            }

            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "分配成功" });
        }
    }
}
