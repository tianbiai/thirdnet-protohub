namespace ProtoHub.Database.Models;

/// <summary>
/// 角色系统菜单关联模型
/// </summary>
public class RoleSystemMenuModel
{
    /// <summary>
    /// 角色ID
    /// </summary>
    public long role_id { get; set; }

    /// <summary>
    /// 系统菜单ID
    /// </summary>
    public long system_menu_id { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime create_time { get; set; }
}
