namespace ProtoHub.Identity.Database.Models;

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
    /// 项目ID（对应 MenuGroup.id）
    /// </summary>
    public long project_id { get; set; }

    /// <summary>
    /// 访问类型（view/manage）
    /// </summary>
    public string access_type { get; set; } = "view";

    /// <summary>
    /// 授权人ID
    /// </summary>
    public long? granted_by { get; set; }

    /// <summary>
    /// 授权时间
    /// </summary>
    public DateTime create_time { get; set; }

    /// <summary>
    /// 用户（导航属性）
    /// </summary>
    public UserModel User { get; set; } = null!;

    /// <summary>
    /// 项目（导航属性）
    /// </summary>
    public MenuGroupModel Project { get; set; } = null!;
}
