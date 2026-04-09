using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProtoHub.Api.Helpers;
using ProtoHub.Database;
using ThirdNet.Core.AspNetCore;

namespace ProtoHub.Api.Controllers
{
    /// <summary>
    /// 默认账户验证器
    /// </summary>
    public class DefaultAccountValidator : IAccountValidator
    {
        private readonly ProtoHubDbContext _dbContext;

        public DefaultAccountValidator(ProtoHubDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Claim>> Validate(string account, string password, string[] scopes)
        {
            // 查询用户
            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.user_name == account);

            if (user == null)
            {
                throw new WebApiException(System.Net.HttpStatusCode.Unauthorized, "用户名或密码错误");
            }

            // 使用 BCrypt 验证密码
            if (!PasswordHelper.VerifyPassword(password, user.password))
            {
                throw new WebApiException(System.Net.HttpStatusCode.Unauthorized, "用户名或密码错误");
            }

            // 检查用户状态
            if (user.status != 1)
            {
                throw new WebApiException(System.Net.HttpStatusCode.Forbidden, "账户已被禁用");
            }

            // 查询用户权限：通过 UserRole -> RolePermission -> Permission 获取
            var permissionCodes = await (from ur in _dbContext.UserRoles
                                         join rp in _dbContext.RolePermissions on ur.role_id equals rp.role_id
                                         join p in _dbContext.Permissions on rp.permission_id equals p.id
                                         where ur.user_id == user.id
                                         select p.code).Distinct().ToListAsync();

            // 创建自定义 claims（只放权限，不放角色）
            var custom_claims = new List<Claim>
            {
                new Claim("idp", "protohub"),
                new Claim(ClaimTypes.NameIdentifier, user.id.ToString()),
                new Claim(ClaimTypes.Name, user.user_name),
            };

            // 添加权限 claims（不添加角色 claims）
            foreach (var permission in permissionCodes)
            {
                custom_claims.Add(new Claim("permission", permission));
            }

            return custom_claims;
        }
    }
}
