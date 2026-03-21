using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProtoHub.Identity.API.Map;
using ProtoHub.Identity.Database;
using ProtoHub.Identity.Database.Models;

namespace ProtoHub.Identity.API.Controllers.Manager;

/// <summary>
/// 角色管理控制器（管理端）
/// </summary>
[ApiController]
[Route("api/manager/role")]
[Authorize]
public class RoleController : ControllerBase
{
    private readonly ProtoHubDbContext _dbContext;
    private readonly ILogger<RoleController> _logger;

    public RoleController(
        ProtoHubDbContext dbContext,
        ILogger<RoleController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// 获取角色列表
    /// </summary>
    [HttpPost("list")]
    public async Task<ActionResult<List<RoleItem>>> List([FromBody] RoleListRequest? request)
    {
        var query = _dbContext.Roles.AsQueryable();

        // 搜索条件
        if (request != null && !string.IsNullOrEmpty(request.Name))
        {
            query = query.Where(r => r.name.Contains(request.Name));
        }

        var roles = await query
            .OrderBy(r => r.id)
            .ToListAsync();

        var items = roles.Select(r => new RoleItem
        {
            Id = r.id,
            Code = r.code,
            Name = r.name,
            Description = r.description,
            IsSystem = r.is_system,
            CreateTime = r.create_time,
            UpdateTime = r.update_time
        }).ToList();

        return Ok(items);
    }

    /// <summary>
    /// 获取角色详情（包含权限）
    /// </summary>
    [HttpPost("{id}/detail")]
    public async Task<ActionResult<RoleDetail>> Detail(long id)
    {
        var role = await _dbContext.Roles
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.id == id);

        if (role == null)
        {
            return NotFound(new ErrorResponse
            {
                Code = "RESOURCE_NOT_FOUND",
                Message = "角色不存在"
            });
        }

        return Ok(new RoleDetail
        {
            Id = role.id,
            Code = role.code,
            Name = role.name,
            Description = role.description,
            IsSystem = role.is_system,
            CreateTime = role.create_time,
            UpdateTime = role.update_time,
            Permissions = role.RolePermissions.Select(rp => new PermissionItem
            {
                Id = rp.Permission.id,
                Code = rp.Permission.code,
                Name = rp.Permission.name,
                Category = rp.Permission.category,
                Description = rp.Permission.description
            }).ToList()
        });
    }

    /// <summary>
    /// 创建角色
    /// </summary>
    [HttpPost("create")]
    public async Task<ActionResult<RoleItem>> Create([FromBody] CreateRoleRequest request)
    {
        // 检查角色编码是否已存在
        if (await _dbContext.Roles.AnyAsync(r => r.code == request.Code))
        {
            return BadRequest(new ErrorResponse
            {
                Code = "ROLE_CODE_EXISTS",
                Message = "角色编码已存在"
            });
        }

        var role = new RoleModel
        {
            code = request.Code,
            name = request.Name,
            description = request.Description,
            is_system = false
        };

        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        return Ok(new RoleItem
        {
            Id = role.id,
            Code = role.code,
            Name = role.name,
            Description = role.description,
            IsSystem = role.is_system,
            CreateTime = role.create_time,
            UpdateTime = role.update_time
        });
    }

    /// <summary>
    /// 更新角色
    /// </summary>
    [HttpPost("update")]
    public async Task<ActionResult<RoleItem>> Update([FromBody] UpdateRoleRequest request)
    {
        var role = await _dbContext.Roles.FindAsync(request.Id);
        if (role == null)
        {
            return NotFound(new ErrorResponse
            {
                Code = "RESOURCE_NOT_FOUND",
                Message = "角色不存在"
            });
        }

        role.name = request.Name;
        role.description = request.Description;

        await _dbContext.SaveChangesAsync();

        return Ok(new RoleItem
        {
            Id = role.id,
            Code = role.code,
            Name = role.name,
            Description = role.description,
            IsSystem = role.is_system,
            CreateTime = role.create_time,
            UpdateTime = role.update_time
        });
    }

    /// <summary>
    /// 删除角色
    /// </summary>
    [HttpPost("delete")]
    public async Task<ActionResult> Delete([FromBody] DeleteRequest request)
    {
        var role = await _dbContext.Roles.FindAsync(request.Id);
        if (role == null)
        {
            return NotFound(new ErrorResponse
            {
                Code = "RESOURCE_NOT_FOUND",
                Message = "角色不存在"
            });
        }

        // 系统内置角色不可删除
        if (role.is_system)
        {
            return BadRequest(new ErrorResponse
            {
                Code = "CANNOT_DELETE_SYSTEM_ROLE",
                Message = "系统内置角色不可删除"
            });
        }

        // 检查是否有用户使用此角色
        var hasUsers = await _dbContext.UserRoles.AnyAsync(ur => ur.role_id == request.Id);
        if (hasUsers)
        {
            return BadRequest(new ErrorResponse
            {
                Code = "ROLE_IN_USE",
                Message = "角色正在被使用，无法删除"
            });
        }

        _dbContext.Roles.Remove(role);
        await _dbContext.SaveChangesAsync();

        return Ok(null);
    }

    /// <summary>
    /// 获取角色权限
    /// </summary>
    [HttpPost("{id}/permissions")]
    public async Task<ActionResult<List<PermissionItem>>> GetRolePermissions(long id)
    {
        var rolePermissions = await _dbContext.RolePermissions
            .Include(rp => rp.Permission)
            .Where(rp => rp.role_id == id)
            .ToListAsync();

        var permissions = rolePermissions.Select(rp => new PermissionItem
        {
            Id = rp.Permission.id,
            Code = rp.Permission.code,
            Name = rp.Permission.name,
            Category = rp.Permission.category,
            Description = rp.Permission.description
        }).ToList();

        return Ok(permissions);
    }

    /// <summary>
    /// 分配角色权限
    /// </summary>
    [HttpPost("{id}/assign-permissions")]
    public async Task<ActionResult> AssignPermissions(long id, [FromBody] List<long> permissionIds)
    {
        var role = await _dbContext.Roles.FindAsync(id);
        if (role == null)
        {
            return NotFound(new ErrorResponse
            {
                Code = "RESOURCE_NOT_FOUND",
                Message = "角色不存在"
            });
        }

        // 删除现有权限
        var existingPermissions = await _dbContext.RolePermissions
            .Where(rp => rp.role_id == id)
            .ToListAsync();
        _dbContext.RolePermissions.RemoveRange(existingPermissions);

        // 添加新权限
        foreach (var permissionId in permissionIds)
        {
            _dbContext.RolePermissions.Add(new RolePermissionModel
            {
                role_id = id,
                permission_id = permissionId
            });
        }

        await _dbContext.SaveChangesAsync();

        return Ok(null);
    }
}
