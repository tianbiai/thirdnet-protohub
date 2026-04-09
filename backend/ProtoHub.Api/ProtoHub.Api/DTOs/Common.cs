using System.Collections.Generic;

namespace ProtoHub.Api.DTOs
{
    /// <summary>
    /// 通用分页响应
    /// </summary>
    public class PageResponse<T>
    {
        public int total { get; set; }
        public int page { get; set; }
        public int page_size { get; set; }
        public List<T> list { get; set; } = new();
    }

    /// <summary>
    /// 通用删除请求
    /// </summary>
    public class DeleteRequest
    {
        [System.ComponentModel.DataAnnotations.Required]
        public long id { get; set; }
    }

    /// <summary>
    /// 通用排序请求
    /// </summary>
    public class ReorderRequest
    {
        [System.ComponentModel.DataAnnotations.Required]
        public List<long> ids { get; set; } = new();
    }
}
