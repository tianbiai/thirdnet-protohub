namespace ProtoHub.Identity.Database.Models;

/// <summary>
/// 菜单分组模型
/// </summary>
public class MenuGroupModel
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public long id { get; set; }

    /// <summary>
    /// 分组名称
    /// </summary>
    public string name { get; set; } = null!;

    /// <summary>
    /// 图标（emoji 或图片 URL）
    /// </summary>
    public string? icon { get; set; }

    /// <summary>
    /// 排序序号
    /// </summary>
    public int order { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime create_time { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime update_time { get; set; }
}
