using ProtoHub.Database.Interfaces;

namespace ProtoHub.Database.Models;

/// <summary>
/// 系统功能菜单模型
/// </summary>
public class SystemMenuModel : IHasTimestamps
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public long id { get; set; }

    /// <summary>
    /// 父菜单ID（支持多级菜单）
    /// </summary>
    public long? parent_id { get; set; }

    /// <summary>
    /// 菜单名称
    /// </summary>
    public string name { get; set; } = string.Empty;

    /// <summary>
    /// 菜单编码（唯一）
    /// </summary>
    public string code { get; set; } = string.Empty;

    /// <summary>
    /// 图标
    /// </summary>
    public string? icon { get; set; }

    /// <summary>
    /// 路由路径
    /// </summary>
    public string? path { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int order { get; set; }

    /// <summary>
    /// 是否可见
    /// </summary>
    public bool is_visible { get; set; } = true;

    /// <summary>
    /// 所需权限
    /// </summary>
    public string? permission { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime create_time { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime update_time { get; set; }
}
