using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ThirdNet.Core.AspNetCore;

namespace ProtoHub.Api.Extensions
{
    /// <summary>
    /// DbContext 扩展方法
    /// </summary>
    public static class DbContextExtensions
    {
        /// <summary>
        /// 根据主键查找实体，不存在则抛出 404 异常
        /// </summary>
        public static async Task<T> EnsureExistsAsync<T>(
            this DbSet<T> dbSet,
            long id,
            string entityName = "记录") where T : class
        {
            var entity = await dbSet.FindAsync(id);
            if (entity == null)
            {
                throw new WebApiException(HttpStatusCode.NotFound, $"{entityName}不存在");
            }
            return entity;
        }

        /// <summary>
        /// 根据条件查找实体，不存在则抛出 404 异常
        /// </summary>
        public static async Task<T> EnsureExistsAsync<T>(
            this IQueryable<T> query,
            string entityName = "记录") where T : class
        {
            var entity = await query.FirstOrDefaultAsync();
            if (entity == null)
            {
                throw new WebApiException(HttpStatusCode.NotFound, $"{entityName}不存在");
            }
            return entity;
        }
    }
}
