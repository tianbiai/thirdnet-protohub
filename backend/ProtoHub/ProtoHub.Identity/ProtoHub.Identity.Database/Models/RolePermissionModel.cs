namespace ProtoHub.Identity.Database.Models;

/// <summary>
/// 角色权限关联模型
/// </summary>
public class RolePermissionModel
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public long id { get; set; }

    /// <summary>
    /// 角色ID
    /// </summary>
    public long role_id { get; set; }

    /// <summary>
    /// 权限ID
    /// </summary>
    public long permission_id { get; set; }

    /// <summary>
    /// 角色（导航属性）
    /// </summary>
    public RoleModel Role { get; set; } = null!;

    /// <summary>
    /// 权限（导航属性）
    /// </summary>
    public PermissionModel Permission { get; set; } = null!;
}
