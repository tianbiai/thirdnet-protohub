using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace ProtoHub.Api.Filters
{
    /// <summary>
    /// 权限校验过滤器：检查用户是否拥有指定的 permission code
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Method | System.AttributeTargets.Class, AllowMultiple = false)]
    public class HasPermissionAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _permission;

        public HasPermissionAttribute(string permission)
        {
            _permission = permission;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            if (user.Identity?.IsAuthenticated != true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // 检查用户是否拥有指定的 permission claim
            var hasPermission = user.HasClaim("permission", _permission);
            if (!hasPermission)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
