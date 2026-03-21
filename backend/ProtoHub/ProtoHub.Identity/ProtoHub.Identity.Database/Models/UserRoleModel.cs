namespace ProtoHub.Identity.Database.Models;

/// <summary>
/// 用户角色关联模型
/// </summary>
public class UserRoleModel
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public long id { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public long user_id { get; set; }

    /// <summary>
    /// 角色ID
    /// </summary>
    public long role_id { get; set; }

    /// <summary>
    /// 用户（导航属性）
    /// </summary>
    public UserModel User { get; set; } = null!;

    /// <summary>
    /// 角色（导航属性）
    /// </summary>
    public RoleModel Role { get; set; } = null!;
}
