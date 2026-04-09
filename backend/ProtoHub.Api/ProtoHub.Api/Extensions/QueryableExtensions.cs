using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ProtoHub.Api.Extensions
{
    /// <summary>
    /// IQueryable 分页扩展方法
    /// </summary>
    public static class QueryableExtensions
    {
        /// <summary>
        /// 对查询应用分页并返回分页结果
        /// </summary>
        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
            this IQueryable<T> query,
            int page,
            int pageSize)
        {
            var total = await query.CountAsync();
            var list = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<T>
            {
                total = total,
                page = page,
                page_size = pageSize,
                list = list
            };
        }
    }

    /// <summary>
    /// 通用分页结果
    /// </summary>
    public class PagedResult<T>
    {
        public int total { get; set; }
        public int page { get; set; }
        public int page_size { get; set; }
        public List<T> list { get; set; } = new();
    }
}
