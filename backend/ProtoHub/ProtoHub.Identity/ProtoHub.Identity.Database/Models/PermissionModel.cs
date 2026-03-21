namespace ProtoHub.Identity.Database.Models;

/// <summary>
/// 权限模型
/// </summary>
public class PermissionModel
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public long id { get; set; }

    /// <summary>
    /// 权限编码（如 project:create）
    /// </summary>
    public string code { get; set; } = null!;

    /// <summary>
    /// 权限名称
    /// </summary>
    public string name { get; set; } = null!;

    /// <summary>
    /// 分类（system/project）
    /// </summary>
    public string category { get; set; } = "system";

    /// <summary>
    /// 描述
    /// </summary>
    public string? description { get; set; }

    /// <summary>
    /// 角色权限关联（导航属性）
    /// </summary>
    public ICollection<RolePermissionModel> RolePermissions { get; set; } = [];
}
