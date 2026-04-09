using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProtoHub.Api.DTOs
{
    public class RoleListRequest
    {
        public string? keyword { get; set; }
        [Range(1, int.MaxValue)]
        public int page { get; set; } = 1;
        [Range(1, 100)]
        public int page_size { get; set; } = 20;
    }

    public class RoleListResponse
    {
        public long id { get; set; }
        public string code { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string? description { get; set; }
        public bool is_system { get; set; }
        public DateTime create_time { get; set; }
        public DateTime update_time { get; set; }
    }

    public class RoleSimpleResponse
    {
        public long id { get; set; }
        public string code { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
    }

    public class RoleDetailResponse : RoleListResponse
    {
        public List<PermissionSimpleResponse> permissions { get; set; } = new();
    }

    public class RoleCreateRequest
    {
        [Required, StringLength(100)]
        public string code { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? description { get; set; }
    }

    public class RoleUpdateRequest
    {
        [Required]
        public long id { get; set; }

        [Required, StringLength(100)]
        public string code { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? description { get; set; }
    }

    public class AssignPermissionsRequest
    {
        [Required]
        public List<long> permission_ids { get; set; } = new();
    }

    public class PermissionSimpleResponse
    {
        public long id { get; set; }
        public string code { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string? category { get; set; }
    }
}
