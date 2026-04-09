using ProtoHub.Database.Interfaces;

namespace ProtoHub.Database.Models;

/// <summary>
/// 角色模型
/// </summary>
public class RoleModel : IHasTimestamps
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public long id { get; set; }

    /// <summary>
    /// 角色编码（唯一）
    /// </summary>
    public string code { get; set; } = string.Empty;

    /// <summary>
    /// 角色名称
    /// </summary>
    public string name { get; set; } = string.Empty;

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
}
