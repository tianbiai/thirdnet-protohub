namespace ProtoHub.Database.Models;

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
    /// 权限编码（唯一）
    /// </summary>
    public string code { get; set; } = string.Empty;

    /// <summary>
    /// 权限名称
    /// </summary>
    public string name { get; set; } = string.Empty;

    /// <summary>
    /// 分类
    /// </summary>
    public string? category { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string? description { get; set; }
}
