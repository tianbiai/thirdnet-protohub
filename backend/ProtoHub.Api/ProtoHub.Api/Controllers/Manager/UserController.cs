using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProtoHub.Api.DTOs;
using ProtoHub.Api.Extensions;
using ProtoHub.Api.Filters;
using ProtoHub.Api.Helpers;
using ProtoHub.Database;
using ProtoHub.Database.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThirdNet.Core.AspNetCore;

namespace ProtoHub.Api.Controllers.Manager
{
    /// <summary>
    /// 用户管理控制器
    /// </summary>
    [ApiController]
    [Route("api/manager/user")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly ProtoHubDbContext _dbContext;

        public UserController(ProtoHubDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// 获取用户列表（分页）
        /// </summary>
        [HttpPost("list")]
        [HasPermission("user-manage:view")]
        public async Task<IActionResult> GetList([FromBody] UserListRequest request)
        {
            var query = _dbContext.Users.AsNoTracking();

            // 关键字搜索
            if (!string.IsNullOrEmpty(request.keyword))
            {
                query = query.Where(u => u.user_name.Contains(request.keyword) ||
                                         u.nick_name.Contains(request.keyword) ||
                                         (u.email != null && u.email.Contains(request.keyword)));
            }

            // 状态筛选
            if (request.status.HasValue)
            {
                query = query.Where(u => u.status == request.status.Value);
            }

            var result = await query
                .OrderByDescending(u => u.create_time)
                .ThenByDescending(u => u.id)
                .Select(u => new UserListResponse
                {
                    id = u.id,
                    user_name = u.user_name,
                    nick_name = u.nick_name,
                    email = u.email,
                    status = u.status,
                    description = u.description,
                    create_time = u.create_time,
                    update_time = u.update_time
                })
                .ToPagedResultAsync(request.page, request.page_size);

            return Ok(result);
        }

        /// <summary>
        /// 创建用户
        /// </summary>
        [HttpPost("create")]
        [HasPermission("user-manage:create")]
        public async Task<IActionResult> Create([FromBody] UserCreateRequest request)
        {
            // 检查用户名是否已存在
            var exists = await _dbContext.Users.AnyAsync(u => u.user_name == request.user_name);
            if (exists)
            {
                throw new WebApiException(System.Net.HttpStatusCode.BadRequest, "用户名已存在");
            }

            var user = new UserModel
            {
                user_name = request.user_name,
                password = PasswordHelper.HashPassword(request.password),
                nick_name = request.nick_name ?? request.user_name,
                email = request.email,
                status = request.status ?? 1,
                description = request.description
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            return Ok(new { id = user.id, message = "创建成功" });
        }

        /// <summary>
        /// 更新用户
        /// </summary>
        [HttpPost("update")]
        [HasPermission("user-manage:update")]
        public async Task<IActionResult> Update([FromBody] UserUpdateRequest request)
        {
            var user = await _dbContext.Users.EnsureExistsAsync(request.id, "用户");

            if (request.nick_name != null) user.nick_name = request.nick_name;
            if (request.email != null) user.email = request.email;
            if (request.status.HasValue) user.status = request.status.Value;
            if (request.description != null) user.description = request.description;

            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "更新成功" });
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        [HttpPost("delete")]
        [HasPermission("user-manage:delete")]
        public async Task<IActionResult> Delete([FromBody] DeleteRequest request)
        {
            var user = await _dbContext.Users.EnsureExistsAsync(request.id, "用户");

            // 删除用户角色关联
            var userRoles = await _dbContext.UserRoles.Where(ur => ur.user_id == request.id).ToListAsync();
            _dbContext.UserRoles.RemoveRange(userRoles);

            // 删除用户项目访问
            var userProjects = await _dbContext.UserProjectAccesses.Where(upa => upa.user_id == request.id).ToListAsync();
            _dbContext.UserProjectAccesses.RemoveRange(userProjects);

            // 删除用户
            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "删除成功" });
        }

        /// <summary>
        /// 获取用户角色
        /// </summary>
        [HttpPost("{id}/roles")]
        [HasPermission("user-manage:view")]
        public async Task<IActionResult> GetUserRoles(long id)
        {
            var roles = await (from ur in _dbContext.UserRoles
                               join r in _dbContext.Roles on ur.role_id equals r.id
                               where ur.user_id == id
                               select new RoleResponse
                               {
                                   id = r.id,
                                   code = r.code,
                                   name = r.name,
                                   description = r.description,
                                   is_system = r.is_system
                               }).ToListAsync();

            return Ok(roles);
        }

        /// <summary>
        /// 分配用户角色
        /// </summary>
        [HttpPost("{id}/assign-roles")]
        [HasPermission("user-manage:assign-role")]
        public async Task<IActionResult> AssignRoles(long id, [FromBody] AssignRolesRequest request)
        {
            // 检查用户是否存在
            var user = await _dbContext.Users.AnyAsync(u => u.id == id);
            if (!user)
            {
                throw new WebApiException(System.Net.HttpStatusCode.NotFound, "用户不存在");
            }

            // 删除现有角色
            var existingRoles = await _dbContext.UserRoles.Where(ur => ur.user_id == id).ToListAsync();
            _dbContext.UserRoles.RemoveRange(existingRoles);

            // 添加新角色
            foreach (var roleId in request.role_ids)
            {
                _dbContext.UserRoles.Add(new UserRoleModel
                {
                    user_id = id,
                    role_id = roleId
                });
            }

            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "分配成功" });
        }

        /// <summary>
        /// 重置密码
        /// </summary>
        [HttpPost("reset-password")]
        [HasPermission("user-manage:reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var user = await _dbContext.Users.EnsureExistsAsync(request.id, "用户");

            user.password = PasswordHelper.HashPassword(request.new_password);

            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "密码重置成功" });
        }
    }
}
