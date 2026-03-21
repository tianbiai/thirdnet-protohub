using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProtoHub.Identity.API.Map;
using ProtoHub.Identity.API.Services;
using ProtoHub.Identity.Database;
using ProtoHub.Identity.Database.Models;

namespace ProtoHub.Identity.API.Controllers.Manager;

/// <summary>
/// 用户管理控制器（管理端）
/// </summary>
[ApiController]
[Route("api/manager/user")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly ProtoHubDbContext _dbContext;
    private readonly IPermissionService _permissionService;
    private readonly ILogger<UserController> _logger;

    public UserController(
        ProtoHubDbContext dbContext,
        IPermissionService permissionService,
        ILogger<UserController> logger)
    {
        _dbContext = dbContext;
        _permissionService = permissionService;
        _logger = logger;
    }

    /// <summary>
    /// 获取用户列表
    /// </summary>
    [HttpPost("list")]
    public async Task<ActionResult<PageResponse<UserItem>>> List([FromBody] UserListRequest request)
    {
        var query = _dbContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .AsQueryable();

        // 搜索条件
        if (!string.IsNullOrEmpty(request.Username))
        {
            query = query.Where(u => u.user_name.Contains(request.Username));
        }

        if (!string.IsNullOrEmpty(request.Nickname))
        {
            query = query.Where(u => u.nickname.Contains(request.Nickname));
        }

        if (request.Status.HasValue)
        {
            query = query.Where(u => u.status == request.Status.Value);
        }

        if (!string.IsNullOrEmpty(request.RoleCode))
        {
            query = query.Where(u => u.UserRoles.Any(ur => ur.Role.code == request.RoleCode));
        }

        // 统计总数
        var total = await query.CountAsync();

        // 分页
        var users = await query
            .OrderByDescending(u => u.create_time)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        // 转换为 DTO
        var items = users.Select(u => new UserItem
        {
            Id = u.id,
            Username = u.user_name,
            Nickname = u.nickname,
            Email = u.email,
            Status = u.status,
            Description = u.description,
            Roles = u.UserRoles.Select(ur => new RoleItem
            {
                Id = ur.Role.id,
                Code = ur.Role.code,
                Name = ur.Role.name,
                IsSystem = ur.Role.is_system
            }).ToList(),
            CreateTime = u.create_time,
            UpdateTime = u.update_time
        }).ToList();

        return Ok(new PageResponse<UserItem>
        {
            List = items,
            Total = total,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }

    /// <summary>
    /// 创建用户
    /// </summary>
    [HttpPost("create")]
    public async Task<ActionResult<UserItem>> Create([FromBody] CreateUserRequest request)
    {
        // 检查用户名是否已存在
        if (await _dbContext.Users.AnyAsync(u => u.user_name == request.Username))
        {
            return BadRequest(new ErrorResponse
            {
                Code = "USERNAME_EXISTS",
                Message = "用户名已存在"
            });
        }

        var user = new UserModel
        {
            user_name = request.Username,
            password = HashPassword(request.Password),
            nickname = request.Nickname,
            email = request.Email,
            description = request.Description,
            status = 1
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // 分配角色
        if (request.RoleIds != null && request.RoleIds.Count > 0)
        {
            foreach (var roleId in request.RoleIds)
            {
                _dbContext.UserRoles.Add(new UserRoleModel
                {
                    user_id = user.id,
                    role_id = roleId
                });
            }
            await _dbContext.SaveChangesAsync();
        }

        // 重新加载用户和角色
        await _dbContext.Entry(user)
            .Collection(u => u.UserRoles)
            .Query()
            .Include(ur => ur.Role)
            .LoadAsync();

        return Ok(new UserItem
        {
            Id = user.id,
            Username = user.user_name,
            Nickname = user.nickname,
            Email = user.email,
            Status = user.status,
            Description = user.description,
            Roles = user.UserRoles.Select(ur => new RoleItem
            {
                Id = ur.Role.id,
                Code = ur.Role.code,
                Name = ur.Role.name,
                IsSystem = ur.Role.is_system
            }).ToList(),
            CreateTime = user.create_time,
            UpdateTime = user.update_time
        });
    }

    /// <summary>
    /// 更新用户
    /// </summary>
    [HttpPost("update")]
    public async Task<ActionResult<UserItem>> Update([FromBody] UpdateUserRequest request)
    {
        var user = await _dbContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.id == request.Id);

        if (user == null)
        {
            return NotFound(new ErrorResponse
            {
                Code = "RESOURCE_NOT_FOUND",
                Message = "用户不存在"
            });
        }

        user.nickname = request.Nickname;
        user.email = request.Email;
        user.description = request.Description;

        if (request.Status.HasValue)
        {
            user.status = request.Status.Value;
        }

        await _dbContext.SaveChangesAsync();

        return Ok(new UserItem
        {
            Id = user.id,
            Username = user.user_name,
            Nickname = user.nickname,
            Email = user.email,
            Status = user.status,
            Description = user.description,
            Roles = user.UserRoles.Select(ur => new RoleItem
            {
                Id = ur.Role.id,
                Code = ur.Role.code,
                Name = ur.Role.name,
                IsSystem = ur.Role.is_system
            }).ToList(),
            CreateTime = user.create_time,
            UpdateTime = user.update_time
        });
    }

    /// <summary>
    /// 删除用户
    /// </summary>
    [HttpPost("delete")]
    public async Task<ActionResult> Delete([FromBody] DeleteRequest request)
    {
        var user = await _dbContext.Users.FindAsync(request.Id);
        if (user == null)
        {
            return NotFound(new ErrorResponse
            {
                Code = "RESOURCE_NOT_FOUND",
                Message = "用户不存在"
            });
        }

        // 不允许删除系统管理员
        var isAdmin = await _dbContext.UserRoles
            .AnyAsync(ur => ur.user_id == request.Id && ur.Role.code == "admin");

        if (isAdmin)
        {
            return BadRequest(new ErrorResponse
            {
                Code = "CANNOT_DELETE_ADMIN",
                Message = "不能删除系统管理员"
            });
        }

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();

        return Ok(null);
    }

    /// <summary>
    /// 获取用户角色
    /// </summary>
    [HttpPost("{id}/roles")]
    public async Task<ActionResult<List<RoleItem>>> GetUserRoles(long id)
    {
        var userRoles = await _dbContext.UserRoles
            .Include(ur => ur.Role)
            .Where(ur => ur.user_id == id)
            .ToListAsync();

        var roles = userRoles.Select(ur => new RoleItem
        {
            Id = ur.Role.id,
            Code = ur.Role.code,
            Name = ur.Role.name,
            Description = ur.Role.description,
            IsSystem = ur.Role.is_system,
            CreateTime = ur.Role.create_time,
            UpdateTime = ur.Role.update_time
        }).ToList();

        return Ok(roles);
    }

    /// <summary>
    /// 分配用户角色
    /// </summary>
    [HttpPost("{id}/assign-roles")]
    public async Task<ActionResult> AssignRoles(long id, [FromBody] List<long> roleIds)
    {
        var user = await _dbContext.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound(new ErrorResponse
            {
                Code = "RESOURCE_NOT_FOUND",
                Message = "用户不存在"
            });
        }

        // 删除现有角色
        var existingRoles = await _dbContext.UserRoles
            .Where(ur => ur.user_id == id)
            .ToListAsync();
        _dbContext.UserRoles.RemoveRange(existingRoles);

        // 添加新角色
        foreach (var roleId in roleIds)
        {
            _dbContext.UserRoles.Add(new UserRoleModel
            {
                user_id = id,
                role_id = roleId
            });
        }

        await _dbContext.SaveChangesAsync();

        return Ok(null);
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
