namespace ProtoHub.Database.Models;

/// <summary>
/// 角色权限关联模型
/// </summary>
public class RolePermissionModel
{
    /// <summary>
    /// 角色ID
    /// </summary>
    public long role_id { get; set; }

    /// <summary>
    /// 权限ID
    /// </summary>
    public long permission_id { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime create_time { get; set; }
}
