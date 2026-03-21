using System.ComponentModel.DataAnnotations;

namespace ProtoHub.Identity.API.Map;

#region 角色管理 DTO

/// <summary>
/// 角色列表请求 DTO
/// </summary>
public class RoleListRequest
{
    /// <summary>
    /// 角色名称（模糊搜索）
    /// </summary>
    public string? Name { get; set; }
}

/// <summary>
/// 角色项 DTO
/// </summary>
public class RoleItem
{
    /// <summary>
    /// 角色 ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 角色编码
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// 角色名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 是否系统内置
    /// </summary>
    public bool IsSystem { get; set; }

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
/// 角色详情 DTO（包含权限）
/// </summary>
public class RoleDetail : RoleItem
{
    /// <summary>
    /// 权限列表
    /// </summary>
    public List<PermissionItem> Permissions { get; set; } = [];
}

/// <summary>
/// 创建角色请求 DTO
/// </summary>
public class CreateRoleRequest
{
    /// <summary>
    /// 角色编码
    /// </summary>
    [Required(ErrorMessage = "角色编码不能为空")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "角色编码长度必须在2-50之间")]
    public string Code { get; set; } = null!;

    /// <summary>
    /// 角色名称
    /// </summary>
    [Required(ErrorMessage = "角色名称不能为空")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// 更新角色请求 DTO
/// </summary>
public class UpdateRoleRequest
{
    /// <summary>
    /// 角色ID
    /// </summary>
    [Required(ErrorMessage = "角色ID不能为空")]
    public long Id { get; set; }

    /// <summary>
    /// 角色名称
    /// </summary>
    [Required(ErrorMessage = "角色名称不能为空")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// 分配角色权限请求 DTO
/// </summary>
public class AssignRolePermissionsRequest
{
    /// <summary>
    /// 角色ID
    /// </summary>
    [Required(ErrorMessage = "角色ID不能为空")]
    public long RoleId { get; set; }

    /// <summary>
    /// 权限ID列表
    /// </summary>
    public List<long> PermissionIds { get; set; } = [];
}

#endregion

#region 权限 DTO

/// <summary>
/// 权限项 DTO
/// </summary>
public class PermissionItem
{
    /// <summary>
    /// 权限 ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 权限编码
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// 权限名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 分类
    /// </summary>
    public string Category { get; set; } = null!;

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// 权限列表请求 DTO
/// </summary>
public class PermissionListRequest
{
    /// <summary>
    /// 分类筛选
    /// </summary>
    public string? Category { get; set; }
}

#endregion
