namespace ProtoHub.Identity.Database.Models;

/// <summary>
/// 视口配置（用于小程序等类型）
/// </summary>
public class ViewportConfig
{
    /// <summary>
    /// 视口宽度
    /// </summary>
    public int Width { get; set; } = 375;

    /// <summary>
    /// 视口高度
    /// </summary>
    public int Height { get; set; } = 667;

    /// <summary>
    /// 是否允许缩放
    /// </summary>
    public bool Scalable { get; set; } = false;

    /// <summary>
    /// 设备类型（phone/tablet/desktop）
    /// </summary>
    public string? Device { get; set; }
}
