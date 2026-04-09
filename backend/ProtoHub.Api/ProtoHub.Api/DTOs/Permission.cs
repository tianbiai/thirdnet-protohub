using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProtoHub.Api.DTOs
{
    public class PermissionListRequest
    {
        public string? keyword { get; set; }
        public string? category { get; set; }
        [Range(1, int.MaxValue)]
        public int page { get; set; } = 1;
        [Range(1, 100)]
        public int page_size { get; set; } = 50;
    }

    public class PermissionListResponse
    {
        public long id { get; set; }
        public string code { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string? category { get; set; }
        public string? description { get; set; }
    }

    public class PermissionGroupResponse
    {
        public string category { get; set; } = string.Empty;
        public List<PermissionSimpleResponse> permissions { get; set; } = new();
    }
}
