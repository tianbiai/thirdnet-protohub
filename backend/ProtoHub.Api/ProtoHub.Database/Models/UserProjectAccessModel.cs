namespace ProtoHub.Database.Models;

/// <summary>
/// 用户项目访问模型
/// </summary>
public class UserProjectAccessModel
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public long id { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public long user_id { get; set; }

    /// <summary>
    /// 项目ID（关联 MenuGroupModel.id）
    /// </summary>
    public long project_id { get; set; }

    /// <summary>
    /// 访问类型（view=查看, manage=管理）
    /// </summary>
    public string access_type { get; set; } = "view";

    /// <summary>
    /// 授权人ID
    /// </summary>
    public long granted_by { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime create_time { get; set; }
}
