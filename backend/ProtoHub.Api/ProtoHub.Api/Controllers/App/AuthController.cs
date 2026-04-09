using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProtoHub.Api.DTOs;
using ProtoHub.Api.Extensions;
using ProtoHub.Api.Helpers;
using ProtoHub.Database;
using System;
using System.Linq;
using System.Threading.Tasks;
using ThirdNet.Core.AspNetCore;

namespace ProtoHub.Api.Controllers.App
{
    /// <summary>
    /// 认证控制器
    /// </summary>
    [ApiController]
    [Route("api/app/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ProtoHubDbContext _dbContext;

        public AuthController(ProtoHubDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// 用户登录（已由 JwtTokenController 处理，此接口返回登录信息提示）
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            return Ok(new { message = "请使用 /connect/token 端点进行登录", endpoint = "/connect/token" });
        }

        /// <summary>
        /// 用户登出
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            // JWT 是无状态的，登出主要由客户端清除 token
            // 可以在这里添加 token 黑名单逻辑（如果需要）
            return Ok(new { message = "登出成功" });
        }

        /// <summary>
        /// 获取当前用户信息
        /// </summary>
        [HttpPost("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            // 获取用户ID
            var userId = User.GetUserId();

            // 查询用户信息
            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.id == userId);

            if (user == null)
            {
                throw new WebApiException(System.Net.HttpStatusCode.NotFound, "用户不存在");
            }

            // 查询用户角色
            var roles = await (from ur in _dbContext.UserRoles
                               join r in _dbContext.Roles on ur.role_id equals r.id
                               where ur.user_id == userId
                               select new { r.id, r.code, r.name }).ToListAsync();

            // 查询用户权限
            var permissions = await (from ur in _dbContext.UserRoles
                                     join rp in _dbContext.RolePermissions on ur.role_id equals rp.role_id
                                     join p in _dbContext.Permissions on rp.permission_id equals p.id
                                     where ur.user_id == userId
                                     select new { p.id, p.code, p.name, p.category })
                                     .Distinct()
                                     .ToListAsync();

            // 查询用户可访问的项目
            var projects = await (from upa in _dbContext.UserProjectAccesses
                                  join mg in _dbContext.MenuGroups on upa.project_id equals mg.id
                                  where upa.user_id == userId
                                  select new { mg.id, mg.name, upa.access_type })
                                  .ToListAsync();

            return Ok(new CurrentUserResponse
            {
                id = user.id,
                user_name = user.user_name,
                nick_name = user.nick_name,
                email = user.email,
                status = user.status,
                description = user.description,
                roles = roles.Select(r => new RoleItem
                {
                    id = r.id,
                    code = r.code,
                    name = r.name
                }).ToList(),
                permissions = permissions.Select(p => new PermissionItem
                {
                    id = p.id,
                    code = p.code,
                    name = p.name,
                    category = p.category
                }).ToList(),
                projects = projects.Select(p => new ProjectItem
                {
                    id = p.id,
                    name = p.name,
                    access_type = p.access_type
                }).ToList()
            });
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            // 获取用户ID
            var userId = User.GetUserId();

            // 查询用户
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.id == userId);
            if (user == null)
            {
                throw new WebApiException(System.Net.HttpStatusCode.NotFound, "用户不存在");
            }

            // 验证旧密码
            if (!PasswordHelper.VerifyPassword(request.old_password, user.password))
            {
                throw new WebApiException(System.Net.HttpStatusCode.BadRequest, "原密码错误");
            }

            // 更新密码（使用 BCrypt 哈希）
            user.password = PasswordHelper.HashPassword(request.new_password);

            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "密码修改成功" });
        }
    }
}
