using System;
using System.Net;
using System.Security.Claims;
using ThirdNet.Core.AspNetCore;

namespace ProtoHub.Api.Extensions
{
    /// <summary>
    /// ClaimsPrincipal 扩展方法
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// 从 Claims 中获取当前用户 ID
        /// </summary>
        /// <exception cref="WebApiException">当用户未授权时抛出 401</exception>
        public static long GetUserId(this ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
            {
                throw new WebApiException(HttpStatusCode.Unauthorized, "未授权");
            }
            return userId;
        }

        /// <summary>
        /// 尝试从 Claims 中获取当前用户 ID，失败返回 null
        /// </summary>
        public static long? TryGetUserId(this ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && long.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }
            return null;
        }
    }
}
