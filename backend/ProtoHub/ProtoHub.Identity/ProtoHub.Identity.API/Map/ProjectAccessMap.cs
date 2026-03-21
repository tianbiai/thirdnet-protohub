using System.ComponentModel.DataAnnotations;

namespace ProtoHub.Identity.API.Map;

#region 项目授权 DTO

/// <summary>
/// 项目授权列表请求 DTO
/// </summary>
public class ProjectAccessListRequest : PageRequest
{
    /// <summary>
    /// 项目ID
    /// </summary>
    public long? ProjectId { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public long? UserId { get; set; }

    /// <summary>
    /// 访问类型
    /// </summary>
    public string? AccessType { get; set; }
}

/// <summary>
/// 项目授权项 DTO
/// </summary>
public class ProjectAccessItem
{
    /// <summary>
    /// ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// 用户昵称
    /// </summary>
    public string UserNickname { get; set; } = null!;

    /// <summary>
    /// 项目ID
    /// </summary>
    public long ProjectId { get; set; }

    /// <summary>
    /// 项目名称
    /// </summary>
    public string ProjectName { get; set; } = null!;

    /// <summary>
    /// 访问类型（view/manage）
    /// </summary>
    public string AccessType { get; set; } = null!;

    /// <summary>
    /// 授权人ID
    /// </summary>
    public long? GrantedBy { get; set; }

    /// <summary>
    /// 授权人名称
    /// </summary>
    public string? GrantedByName { get; set; }

    /// <summary>
    /// 授权时间
    /// </summary>
    public DateTime CreateTime { get; set; }
}

/// <summary>
/// 授予项目访问权限请求 DTO
/// </summary>
public class GrantProjectAccessRequest
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [Required(ErrorMessage = "用户ID不能为空")]
    public long UserId { get; set; }

    /// <summary>
    /// 项目ID
    /// </summary>
    [Required(ErrorMessage = "项目ID不能为空")]
    public long ProjectId { get; set; }

    /// <summary>
    /// 访问类型（view/manage）
    /// </summary>
    [Required(ErrorMessage = "访问类型不能为空")]
    public string AccessType { get; set; } = "view";
}

/// <summary>
/// 撤销项目访问权限请求 DTO
/// </summary>
public class RevokeProjectAccessRequest
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [Required(ErrorMessage = "用户ID不能为空")]
    public long UserId { get; set; }

    /// <summary>
    /// 项目ID
    /// </summary>
    [Required(ErrorMessage = "项目ID不能为空")]
    public long ProjectId { get; set; }
}

/// <summary>
/// 用户项目访问列表请求 DTO
/// </summary>
public class UserProjectAccessListRequest
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [Required(ErrorMessage = "用户ID不能为空")]
    public long UserId { get; set; }
}

/// <summary>
/// 用户项目访问项 DTO
/// </summary>
public class UserProjectAccessItem
{
    /// <summary>
    /// ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 项目ID
    /// </summary>
    public long ProjectId { get; set; }

    /// <summary>
    /// 项目名称
    /// </summary>
    public string ProjectName { get; set; } = null!;

    /// <summary>
    /// 项目图标
    /// </summary>
    public string? ProjectIcon { get; set; }

    /// <summary>
    /// 访问类型（view/manage）
    /// </summary>
    public string AccessType { get; set; } = null!;

    /// <summary>
    /// 授权时间
    /// </summary>
    public DateTime CreateTime { get; set; }
}

#endregion
