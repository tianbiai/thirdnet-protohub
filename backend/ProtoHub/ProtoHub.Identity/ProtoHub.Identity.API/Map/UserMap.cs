using System.ComponentModel.DataAnnotations;

namespace ProtoHub.Identity.API.Map;

#region 用户管理 DTO

/// <summary>
/// 用户列表请求 DTO
/// </summary>
public class UserListRequest : PageRequest
{
    /// <summary>
    /// 用户名（模糊搜索）
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// 昵称（模糊搜索）
    /// </summary>
    public string? Nickname { get; set; }

    /// <summary>
    /// 状态筛选
    /// </summary>
    public int? Status { get; set; }

    /// <summary>
    /// 角色筛选
    /// </summary>
    public string? RoleCode { get; set; }
}

/// <summary>
/// 用户项 DTO
/// </summary>
public class UserItem
{
    /// <summary>
    /// 用户 ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// 昵称
    /// </summary>
    public string Nickname { get; set; } = null!;

    /// <summary>
    /// 邮箱
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// 状态（0=禁用, 1=启用）
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 角色列表
    /// </summary>
    public List<RoleItem> Roles { get; set; } = [];

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdateTime { get; set; }
}

/// <summary>
/// 创建用户请求 DTO
/// </summary>
public class CreateUserRequest
{
    /// <summary>
    /// 用户名
    /// </summary>
    [Required(ErrorMessage = "用户名不能为空")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "用户名长度必须在2-100之间")]
    public string Username { get; set; } = null!;

    /// <summary>
    /// 密码
    /// </summary>
    [Required(ErrorMessage = "密码不能为空")]
    [MinLength(6, ErrorMessage = "密码长度不能少于6位")]
    public string Password { get; set; } = null!;

    /// <summary>
    /// 昵称
    /// </summary>
    [Required(ErrorMessage = "昵称不能为空")]
    public string Nickname { get; set; } = null!;

    /// <summary>
    /// 邮箱
    /// </summary>
    [EmailAddress(ErrorMessage = "邮箱格式不正确")]
    public string? Email { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 角色ID列表
    /// </summary>
    public List<long>? RoleIds { get; set; }
}

/// <summary>
/// 更新用户请求 DTO
/// </summary>
public class UpdateUserRequest
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [Required(ErrorMessage = "用户ID不能为空")]
    public long Id { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    [Required(ErrorMessage = "昵称不能为空")]
    public string Nickname { get; set; } = null!;

    /// <summary>
    /// 邮箱
    /// </summary>
    [EmailAddress(ErrorMessage = "邮箱格式不正确")]
    public string? Email { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public int? Status { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// 分配用户角色请求 DTO
/// </summary>
public class AssignUserRolesRequest
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [Required(ErrorMessage = "用户ID不能为空")]
    public long UserId { get; set; }

    /// <summary>
    /// 角色ID列表
    /// </summary>
    public List<long> RoleIds { get; set; } = [];
}

#endregion
