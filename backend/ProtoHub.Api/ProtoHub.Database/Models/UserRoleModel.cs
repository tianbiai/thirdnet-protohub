namespace ProtoHub.Database.Models;

/// <summary>
/// 用户角色关联模型
/// </summary>
public class UserRoleModel
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public long user_id { get; set; }

    /// <summary>
    /// 角色ID
    /// </summary>
    public long role_id { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime create_time { get; set; }
}
