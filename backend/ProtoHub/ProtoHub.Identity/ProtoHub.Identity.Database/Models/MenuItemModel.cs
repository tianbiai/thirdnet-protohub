namespace ProtoHub.Identity.Database.Models;

/// <summary>
/// 菜单项模型
/// </summary>
public class MenuItemModel
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public long id { get; set; }

    /// <summary>
    /// 所属分组ID
    /// </summary>
    public long group_id { get; set; }

    /// <summary>
    /// 菜单项名称
    /// </summary>
    public string name { get; set; } = null!;

    /// <summary>
    /// 图标（emoji 或图片 URL）
    /// </summary>
    public string? icon { get; set; }

    /// <summary>
    /// 类型（web/miniprogram/doc/swagger/internal）
    /// </summary>
    public string type { get; set; } = null!;

    /// <summary>
    /// 外部链接地址
    /// </summary>
    public string? url { get; set; }

    /// <summary>
    /// 描述信息
    /// </summary>
    public string? description { get; set; }

    /// <summary>
    /// 排序序号
    /// </summary>
    public int order { get; set; }

    /// <summary>
    /// 视口配置（type 为 miniprogram 时使用）
    /// </summary>
    public ViewportConfig? viewport { get; set; }

    /// <summary>
    /// 文档文件ID（type 为 doc 时使用）
    /// </summary>
    public string? doc_file_id { get; set; }

    /// <summary>
    /// 文档文件名
    /// </summary>
    public string? doc_file_name { get; set; }

    /// <summary>
    /// 文档描述
    /// </summary>
    public string? doc_description { get; set; }

    /// <summary>
    /// 内部路由路径（type 为 internal 时使用）
    /// </summary>
    public string? route { get; set; }

    /// <summary>
    /// 访问所需权限标识
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
