using System;

namespace ProtoHub.Database.Interfaces
{
    /// <summary>
    /// 标记实体包含时间戳字段，用于自动更新 create_time 和 update_time
    /// </summary>
    public interface IHasTimestamps
    {
        DateTime create_time { get; set; }
        DateTime update_time { get; set; }
    }
}
