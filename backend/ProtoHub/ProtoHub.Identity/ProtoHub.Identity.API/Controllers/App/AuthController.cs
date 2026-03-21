using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProtoHub.Identity.API.Map;
using ProtoHub.Identity.API.Services;
using ProtoHub.Identity.Database;
using ProtoHub.Identity.Database.Models;
using ThirdNet.Core.AspNetCore;

namespace ProtoHub.Identity.API.Controllers.App;

/// <summary>
/// 认证控制器（应用端）
/// </summary>
[ApiController]
[Route("api/app/auth")]
public class AuthController : ControllerBase
{
    private readonly ProtoHubDbContext _dbContext;
    private readonly JwtTokenManager _tokenManager;
    private readonly IPermissionService _permissionService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        ProtoHubDbContext dbContext,
        JwtTokenManager tokenManager,
        IPermissionService permissionService,
        ILogger<AuthController> logger)
    {
        _dbContext = dbContext;
        _tokenManager = tokenManager;
        _permissionService = permissionService;
        _logger = logger;
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        // 查找用户
        var user = await _dbContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.user_name == request.Username);

        if (user == null || !VerifyPassword(request.Password, user.password))
        {
            return Unauthorized(new ErrorResponse
            {
                Code = "INVALID_CREDENTIALS",
                Message = "用户名或密码错误"
            });
        }

        // 检查用户状态
        if (user.status != 1)
        {
            return Unauthorized(new ErrorResponse
            {
                Code = "USER_DISABLED",
                Message = "用户已被禁用"
            });
        }

        // 获取角色编码列表
        var roleCodes = user.UserRoles.Select(ur => ur.Role.code).ToList();

        // 创建 claims
        var claims = new List<System.Security.Claims.Claim>
        {
            new(System.Security.Claims.ClaimTypes.NameIdentifier, user.id.ToString()),
            new(System.Security.Claims.ClaimTypes.Name, user.user_name)
        };

        // 添加角色 claims
        foreach (var roleCode in roleCodes)
        {
            claims.Add(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, roleCode));
        }

        // 生成 JWT Token
        var token = await _tokenManager.CreateAccessToken(
            user.user_name,
            "protohub",
            claims);

        // 获取用户权限
        var permissions = await _permissionService.GetUserPermissionCodesAsync(user.id);
        var accessibleProjects = await _permissionService.GetAccessibleProjectIdsAsync(user.id);
        var manageableProjects = await _permissionService.GetManageableProjectIdsAsync(user.id);

        return Ok(new LoginResponse
        {
            Token = token,
            UserInfo = new UserInfo
            {
                Id = user.id,
                Username = user.user_name,
                Nickname = user.nickname,
                Email = user.email,
                Roles = roleCodes,
                Permissions = permissions.ToList(),
                AccessibleProjects = accessibleProjects.ToList(),
                ManageableProjects = manageableProjects.ToList()
            }
        });
    }

    /// <summary>
    /// 用户登出
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public ActionResult Logout()
    {
        // JWT 无状态，登出由客户端处理（清除本地 token）
        return Ok(null);
    }

    /// <summary>
    /// 获取当前用户信息
    /// </summary>
    [HttpPost("me")]
    [Authorize]
    public async Task<ActionResult<CurrentUserResponse>> Me()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized(new ErrorResponse
            {
                Code = "TOKEN_INVALID",
                Message = "Token 无效"
            });
        }

        var user = await _dbContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.id == userId);

        if (user == null)
        {
            return NotFound(new ErrorResponse
            {
                Code = "RESOURCE_NOT_FOUND",
                Message = "用户不存在"
            });
        }

        // 获取角色编码列表
        var roleCodes = user.UserRoles.Select(ur => ur.Role.code).ToList();

        // 获取用户权限
        var permissions = await _permissionService.GetUserPermissionCodesAsync(userId);
        var accessibleProjects = await _permissionService.GetAccessibleProjectIdsAsync(userId);
        var manageableProjects = await _permissionService.GetManageableProjectIdsAsync(userId);

        return Ok(new CurrentUserResponse
        {
            Id = user.id,
            Username = user.user_name,
            Nickname = user.nickname,
            Email = user.email,
            Roles = roleCodes,
            Permissions = permissions.ToList(),
            AccessibleProjects = accessibleProjects.ToList(),
            ManageableProjects = manageableProjects.ToList()
        });
    }

    /// <summary>
    /// 修改密码
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized(new ErrorResponse
            {
                Code = "TOKEN_INVALID",
                Message = "Token 无效"
            });
        }

        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound(new ErrorResponse
            {
                Code = "RESOURCE_NOT_FOUND",
                Message = "用户不存在"
            });
        }

        // 验证旧密码
        if (!VerifyPassword(request.OldPassword, user.password))
        {
            return BadRequest(new ErrorResponse
            {
                Code = "INVALID_OLD_PASSWORD",
                Message = "旧密码错误"
            });
        }

        // 更新密码
        user.password = HashPassword(request.NewPassword);
        await _dbContext.SaveChangesAsync();

        return Ok(null);
    }

    /// <summary>
    /// 验证密码
    /// </summary>
    private static bool VerifyPassword(string inputPassword, string storedPassword)
    {
        // 简单对比，生产环境应使用 BCrypt.Verify
        return inputPassword == storedPassword;
    }

    /// <summary>
    /// 哈希密码
    /// </summary>
    private static string HashPassword(string password)
    {
        // 简单存储，生产环境应使用 BCrypt.HashPassword
        return password;
    }
}
