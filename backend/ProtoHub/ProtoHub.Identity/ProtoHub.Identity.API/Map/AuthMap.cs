using System.ComponentModel.DataAnnotations;

namespace ProtoHub.Identity.API.Map;

#region 认证相关 DTO

/// <summary>
/// 登录请求 DTO
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// 用户名
    /// </summary>
    [Required(ErrorMessage = "用户名不能为空")]
    public string Username { get; set; } = null!;

    /// <summary>
    /// 密码
    /// </summary>
    [Required(ErrorMessage = "密码不能为空")]
    public string Password { get; set; } = null!;
}

/// <summary>
/// 登录响应 DTO
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// JWT Token
    /// </summary>
    public string Token { get; set; } = null!;

    /// <summary>
    /// 用户信息
    /// </summary>
    public UserInfo UserInfo { get; set; } = null!;
}

/// <summary>
/// 用户信息 DTO
/// </summary>
public class UserInfo
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
    /// 角色列表
    /// </summary>
    public List<string> Roles { get; set; } = [];

    /// <summary>
    /// 权限编码列表
    /// </summary>
    public List<string> Permissions { get; set; } = [];

    /// <summary>
    /// 可访问的项目ID列表
    /// </summary>
    public List<long> AccessibleProjects { get; set; } = [];

    /// <summary>
    /// 可管理的项目ID列表
    /// </summary>
    public List<long> ManageableProjects { get; set; } = [];
}

/// <summary>
/// 当前用户响应 DTO
/// </summary>
public class CurrentUserResponse
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
    /// 角色列表
    /// </summary>
    public List<string> Roles { get; set; } = [];

    /// <summary>
    /// 权限编码列表
    /// </summary>
    public List<string> Permissions { get; set; } = [];

    /// <summary>
    /// 可访问的项目ID列表
    /// </summary>
    public List<long> AccessibleProjects { get; set; } = [];

    /// <summary>
    /// 可管理的项目ID列表
    /// </summary>
    public List<long> ManageableProjects { get; set; } = [];
}

/// <summary>
/// 修改密码请求 DTO
/// </summary>
public class ChangePasswordRequest
{
    /// <summary>
    /// 旧密码
    /// </summary>
    [Required(ErrorMessage = "旧密码不能为空")]
    public string OldPassword { get; set; } = null!;

    /// <summary>
    /// 新密码
    /// </summary>
    [Required(ErrorMessage = "新密码不能为空")]
    [MinLength(6, ErrorMessage = "密码长度不能少于6位")]
    public string NewPassword { get; set; } = null!;
}

#endregion

#region 通用响应

/// <summary>
/// 错误响应 DTO
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// 错误码
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// 错误消息
    /// </summary>
    public string Message { get; set; } = null!;
}

/// <summary>
/// 删除请求 DTO
/// </summary>
public class DeleteRequest
{
    /// <summary>
    /// ID
    /// </summary>
    [Required(ErrorMessage = "ID不能为空")]
    public long Id { get; set; }
}

/// <summary>
/// 分页请求 DTO
/// </summary>
public class PageRequest
{
    /// <summary>
    /// 页码（从1开始）
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// 每页数量
    /// </summary>
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// 分页响应 DTO
/// </summary>
public class PageResponse<T>
{
    /// <summary>
    /// 数据列表
    /// </summary>
    public List<T> List { get; set; } = [];

    /// <summary>
    /// 总数
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// 当前页码
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// 每页数量
    /// </summary>
    public int PageSize { get; set; }
}

#endregion
