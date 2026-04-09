namespace ProtoHub.Api.Helpers;

/// <summary>
/// 密码工具类，使用 BCrypt 进行密码哈希和验证
/// </summary>
public static class PasswordHelper
{
    private const int WorkFactor = 12;

    /// <summary>
    /// 对密码进行哈希
    /// </summary>
    public static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    /// <summary>
    /// 验证密码是否匹配
    /// </summary>
    public static bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}
