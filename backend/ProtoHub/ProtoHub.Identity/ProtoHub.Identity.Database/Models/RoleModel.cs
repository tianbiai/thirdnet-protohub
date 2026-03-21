namespace ProtoHub.Identity.Database.Models;

/// <summary>
/// 角色模型
/// </summary>
public class RoleModel
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public long id { get; set; }

    /// <summary>
    /// 角色编码（唯一）
    /// </summary>
    public string code { get; set; } = null!;

    /// <summary>
    /// 角色名称
    /// </summary>
    public string name { get; set; } = null!;

    /// <summary>
    /// 描述
    /// </summary>
    public string? description { get; set; }

    /// <summary>
    /// 是否系统内置（不可删除）
    /// </summary>
    public bool is_system { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime create_time { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime update_time { get; set; }

    /// <summary>
    /// 角色权限关联（导航属性）
    /// </summary>
    public ICollection<RolePermissionModel> RolePermissions { get; set; } = [];

    /// <summary>
    /// 用户角色关联（导航属性）
    /// </summary>
    public ICollection<UserRoleModel> UserRoles { get; set; } = [];
}
