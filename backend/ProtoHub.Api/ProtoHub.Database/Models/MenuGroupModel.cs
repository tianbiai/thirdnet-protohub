using ProtoHub.Database.Interfaces;

namespace ProtoHub.Database.Models;

/// <summary>
/// 菜单分组模型（项目表）
/// </summary>
public class MenuGroupModel : IHasTimestamps
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public long id { get; set; }

    /// <summary>
    /// 分组名称
    /// </summary>
    public string name { get; set; } = string.Empty;

    /// <summary>
    /// 图标
    /// </summary>
    public string? icon { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int order { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string? description { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime create_time { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime update_time { get; set; }
}
