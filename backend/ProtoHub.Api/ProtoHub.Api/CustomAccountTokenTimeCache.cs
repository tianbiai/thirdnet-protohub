using System;
using System.Collections.Concurrent;

namespace ThirdNet.Core.AspNetCore
{
    /// <summary>
    /// 账户 Token 时间缓存实现，用于支持 Token 撤销
    /// 存储每个账户的最后有效时间，Token 签发时间早于此时间的视为无效
    /// </summary>
    public class CustomAccountTokenTimeCache : IAccountTokenTimeCache
    {
        private readonly ConcurrentDictionary<string, DateTime> _cache = new();

        /// <summary>
        /// 尝试获取账户的最后有效时间
        /// </summary>
        public bool TryGet(string key, out DateTime time)
        {
            return _cache.TryGetValue(key, out time);
        }

        /// <summary>
        /// 设置账户的最后有效时间（用于 Token 撤销）
        /// </summary>
        public void Set(string key, DateTime time)
        {
            _cache.AddOrUpdate(key, time, (_, _) => time);
        }

        /// <summary>
        /// 移除账户的时间记录
        /// </summary>
        public bool Remove(string key)
        {
            return _cache.TryRemove(key, out _);
        }
    }
}
