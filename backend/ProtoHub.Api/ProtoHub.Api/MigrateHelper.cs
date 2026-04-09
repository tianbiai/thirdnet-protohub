using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProtoHub.Database;

namespace ProtoHub.Api
{
    /// <summary>
    /// 数据库初始化
    /// </summary>
    public static class MigrateHelper
    {
        /// <summary>
        /// 初始化 ProtoHub 数据库
        /// </summary>
        public static void InitializeProtoHubDatabase(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ProtoHubDbContext>();
            dbContext.Database.Migrate();
        }
    }
}
