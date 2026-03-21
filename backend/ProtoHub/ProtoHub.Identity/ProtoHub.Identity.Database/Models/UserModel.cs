namespace ProtoHub.Identity.Database.Models;

/// <summary>
/// 用户模型
/// </summary>
public class UserModel
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public long id { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string user_name { get; set; } = null!;

    /// <summary>
    /// 密码（加密存储）
    /// </summary>
    public string password { get; set; } = null!;

    /// <summary>
    /// 昵称
    /// </summary>
    public string nickname { get; set; } = null!;

    /// <summary>
    /// 邮箱
    /// </summary>
    public string? email { get; set; }

    /// <summary>
    /// 状态（0=禁用, 1=启用）
    /// </summary>
    public int status { get; set; } = 1;

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

    /// <summary>
    /// 用户角色关联（导航属性）
    /// </summary>
    public ICollection<UserRoleModel> UserRoles { get; set; } = [];

    /// <summary>
    /// 用户项目访问关联（导航属性）
    /// </summary>
    public ICollection<UserProjectAccessModel> ProjectAccesses { get; set; } = [];
}
