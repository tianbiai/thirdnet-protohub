using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ProtoHub.Identity.API
{
    /// <summary>
    /// 数据库初始化
    /// </summary>
    public static class MigrateHelper
    {
        /// <summary>
        /// 初始化数据库
        /// </summary>
        public static void InitializeTokenDatabase(this IApplicationBuilder app)
        {
        }
    }
}
