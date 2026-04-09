using ProtoHub.Database.Interfaces;

namespace ProtoHub.Database.Models;

/// <summary>
/// 菜单项模型（项目子项表）
/// </summary>
public class MenuItemModel : IHasTimestamps
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
    /// 菜单名称
    /// </summary>
    public string name { get; set; } = string.Empty;

    /// <summary>
    /// 类型（web/miniprogram/link/changelog）
    /// </summary>
    public string type { get; set; } = "web";

    /// <summary>
    /// 链接地址
    /// </summary>
    public string? url { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string? description { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int order { get; set; }

    /// <summary>
    /// 视口配置（JSONB，用于小程序等）
    /// </summary>
    public ViewportConfigModel? viewport_config { get; set; }

    /// <summary>
    /// 文档文件ID
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
    /// 内部路由
    /// </summary>
    public string? route { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime create_time { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime update_time { get; set; }
}

/// <summary>
/// 视口配置模型（JSONB 嵌套对象）
/// </summary>
public class ViewportConfigModel
{
    /// <summary>
    /// 宽度
    /// </summary>
    public int? width { get; set; }

    /// <summary>
    /// 高度
    /// </summary>
    public int? height { get; set; }

    /// <summary>
    /// 设备类型
    /// </summary>
    public string? device_type { get; set; }

    /// <summary>
    /// 缩放比例
    /// </summary>
    public decimal? scale { get; set; }
}
