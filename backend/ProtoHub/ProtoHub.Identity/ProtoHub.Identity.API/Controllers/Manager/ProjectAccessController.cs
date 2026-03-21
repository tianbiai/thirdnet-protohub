using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProtoHub.Identity.API.Map;
using ProtoHub.Identity.API.Services;
using ProtoHub.Identity.Database;
using ProtoHub.Identity.Database.Models;

namespace ProtoHub.Identity.API.Controllers.Manager;

/// <summary>
/// 项目授权控制器（管理端）
/// </summary>
[ApiController]
[Route("api/manager/project-access")]
[Authorize]
public class ProjectAccessController : ControllerBase
{
    private readonly ProtoHubDbContext _dbContext;
    private readonly IPermissionService _permissionService;
    private readonly ILogger<ProjectAccessController> _logger;

    public ProjectAccessController(
        ProtoHubDbContext dbContext,
        IPermissionService permissionService,
        ILogger<ProjectAccessController> logger)
    {
        _dbContext = dbContext;
        _permissionService = permissionService;
        _logger = logger;
    }

    /// <summary>
    /// 获取项目授权列表
    /// </summary>
    [HttpPost("list")]
    public async Task<ActionResult<PageResponse<ProjectAccessItem>>> List([FromBody] ProjectAccessListRequest request)
    {
        var currentUserId = GetCurrentUserId();
        var isAdmin = await _permissionService.IsAdminAsync(currentUserId);
        var manageableProjects = await _permissionService.GetManageableProjectIdsAsync(currentUserId);

        var query = _dbContext.UserProjectAccesses
            .Include(a => a.User)
            .Include(a => a.Project)
            .AsQueryable();

        // 非管理员只能查看自己管理的项目的授权
        if (!isAdmin)
        {
            query = query.Where(a => manageableProjects.Contains(a.project_id));
        }

        // 筛选条件
        if (request.ProjectId.HasValue)
        {
            // 检查是否有权限查看此项目
            if (!isAdmin && !manageableProjects.Contains(request.ProjectId.Value))
            {
                return Forbid();
            }
            query = query.Where(a => a.project_id == request.ProjectId.Value);
        }

        if (request.UserId.HasValue)
        {
            query = query.Where(a => a.user_id == request.UserId.Value);
        }

        if (!string.IsNullOrEmpty(request.AccessType))
        {
            query = query.Where(a => a.access_type == request.AccessType);
        }

        // 统计总数
        var total = await query.CountAsync();

        // 分页
        var accesses = await query
            .OrderByDescending(a => a.create_time)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        // 获取授权人信息
        var granterIds = accesses
            .Where(a => a.granted_by.HasValue)
            .Select(a => a.granted_by!.Value)
            .Distinct()
            .ToList();

        var granters = await _dbContext.Users
            .Where(u => granterIds.Contains(u.id))
            .ToDictionaryAsync(u => u.id, u => u.nickname);

        // 转换为 DTO
        var items = accesses.Select(a => new ProjectAccessItem
        {
            Id = a.id,
            UserId = a.user_id,
            Username = a.User.user_name,
            UserNickname = a.User.nickname,
            ProjectId = a.project_id,
            ProjectName = a.Project.name,
            AccessType = a.access_type,
            GrantedBy = a.granted_by,
            GrantedByName = a.granted_by.HasValue && granters.TryGetValue(a.granted_by.Value, out var name) ? name : null,
            CreateTime = a.create_time
        }).ToList();

        return Ok(new PageResponse<ProjectAccessItem>
        {
            List = items,
            Total = total,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }

    /// <summary>
    /// 授予项目访问权限
    /// </summary>
    [HttpPost("grant")]
    public async Task<ActionResult<ProjectAccessItem>> Grant([FromBody] GrantProjectAccessRequest request)
    {
        var currentUserId = GetCurrentUserId();
        var isAdmin = await _permissionService.IsAdminAsync(currentUserId);

        // 检查是否可以管理此项目
        if (!isAdmin && !await _permissionService.CanManageProjectAsync(currentUserId, request.ProjectId))
        {
            return Forbid();
        }

        // 检查用户是否存在
        var user = await _dbContext.Users.FindAsync(request.UserId);
        if (user == null)
        {
            return NotFound(new ErrorResponse
            {
                Code = "RESOURCE_NOT_FOUND",
                Message = "用户不存在"
            });
        }

        // 检查项目是否存在
        var project = await _dbContext.MenuGroups.FindAsync(request.ProjectId);
        if (project == null)
        {
            return NotFound(new ErrorResponse
            {
                Code = "RESOURCE_NOT_FOUND",
                Message = "项目不存在"
            });
        }

        // 检查是否已存在授权
        var existingAccess = await _dbContext.UserProjectAccesses
            .FirstOrDefaultAsync(a => a.user_id == request.UserId && a.project_id == request.ProjectId);

        if (existingAccess != null)
        {
            // 更新访问类型
            existingAccess.access_type = request.AccessType;
            existingAccess.granted_by = currentUserId;
            await _dbContext.SaveChangesAsync();

            return Ok(new ProjectAccessItem
            {
                Id = existingAccess.id,
                UserId = existingAccess.user_id,
                Username = user.user_name,
                UserNickname = user.nickname,
                ProjectId = existingAccess.project_id,
                ProjectName = project.name,
                AccessType = existingAccess.access_type,
                GrantedBy = existingAccess.granted_by,
                CreateTime = existingAccess.create_time
            });
        }

        // 创建新授权
        var access = new UserProjectAccessModel
        {
            user_id = request.UserId,
            project_id = request.ProjectId,
            access_type = request.AccessType,
            granted_by = currentUserId
        };

        _dbContext.UserProjectAccesses.Add(access);
        await _dbContext.SaveChangesAsync();

        return Ok(new ProjectAccessItem
        {
            Id = access.id,
            UserId = access.user_id,
            Username = user.user_name,
            UserNickname = user.nickname,
            ProjectId = access.project_id,
            ProjectName = project.name,
            AccessType = access.access_type,
            GrantedBy = access.granted_by,
            CreateTime = access.create_time
        });
    }

    /// <summary>
    /// 撤销项目访问权限
    /// </summary>
    [HttpPost("revoke")]
    public async Task<ActionResult> Revoke([FromBody] RevokeProjectAccessRequest request)
    {
        var currentUserId = GetCurrentUserId();
        var isAdmin = await _permissionService.IsAdminAsync(currentUserId);

        // 检查是否可以管理此项目
        if (!isAdmin && !await _permissionService.CanManageProjectAsync(currentUserId, request.ProjectId))
        {
            return Forbid();
        }

        var access = await _dbContext.UserProjectAccesses
            .FirstOrDefaultAsync(a => a.user_id == request.UserId && a.project_id == request.ProjectId);

        if (access == null)
        {
            return NotFound(new ErrorResponse
            {
                Code = "RESOURCE_NOT_FOUND",
                Message = "授权记录不存在"
            });
        }

        _dbContext.UserProjectAccesses.Remove(access);
        await _dbContext.SaveChangesAsync();

        return Ok(null);
    }

    /// <summary>
    /// 获取用户的项目访问列表
    /// </summary>
    [HttpPost("user/{id}/projects")]
    public async Task<ActionResult<List<UserProjectAccessItem>>> GetUserProjects(long id)
    {
        var accesses = await _dbContext.UserProjectAccesses
            .Include(a => a.Project)
            .Where(a => a.user_id == id)
            .OrderBy(a => a.Project.order)
            .ToListAsync();

        var items = accesses.Select(a => new UserProjectAccessItem
        {
            Id = a.id,
            ProjectId = a.project_id,
            ProjectName = a.Project.name,
            ProjectIcon = a.Project.icon,
            AccessType = a.access_type,
            CreateTime = a.create_time
        }).ToList();

        return Ok(items);
    }

    /// <summary>
    /// 获取当前用户可管理的项目
    /// </summary>
    [HttpPost("my-projects")]
    public async Task<ActionResult<List<UserProjectAccessItem>>> MyProjects()
    {
        var currentUserId = GetCurrentUserId();
        var manageableProjectIds = await _permissionService.GetManageableProjectIdsAsync(currentUserId);

        var projects = await _dbContext.MenuGroups
            .Where(g => manageableProjectIds.Contains(g.id))
            .OrderBy(g => g.order)
            .ToListAsync();

        var items = projects.Select(p => new UserProjectAccessItem
        {
            Id = 0,
            ProjectId = p.id,
            ProjectName = p.name,
            ProjectIcon = p.icon,
            AccessType = "manage",
            CreateTime = DateTime.MinValue
        }).ToList();

        return Ok(items);
    }

    /// <summary>
    /// 获取当前用户 ID
    /// </summary>
    private long GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null && long.TryParse(userIdClaim.Value, out var userId) ? userId : 0;
    }
}
