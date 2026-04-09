using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProtoHub.Api.DTOs
{
    public class SystemMenuListRequest
    {
        public long? parent_id { get; set; }
        public bool? is_visible { get; set; }
        public string? keyword { get; set; }
    }

    public class SystemMenuListResponse
    {
        public long id { get; set; }
        public long? parent_id { get; set; }
        public string name { get; set; } = string.Empty;
        public string code { get; set; } = string.Empty;
        public string? icon { get; set; }
        public string? path { get; set; }
        public int order { get; set; }
        public bool is_visible { get; set; } = true;
        public string? permission { get; set; }
        public DateTime create_time { get; set; }
        public DateTime update_time { get; set; }
    }

    public class SystemMenuTreeResponse : SystemMenuListResponse
    {
        public List<SystemMenuTreeResponse> children { get; set; } = new();
    }

    public class SystemMenuSimpleResponse
    {
        public long id { get; set; }
        public string name { get; set; } = string.Empty;
        public string code { get; set; } = string.Empty;
        public string? icon { get; set; }
        public string? path { get; set; }
    }

    public class CreateSystemMenuRequest
    {
        public long? parent_id { get; set; }

        [Required, StringLength(100)]
        public string name { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string code { get; set; } = string.Empty;

        public string? icon { get; set; }

        [StringLength(200)]
        public string? path { get; set; }

        public int order { get; set; }

        public bool is_visible { get; set; } = true;

        [StringLength(100)]
        public string? permission { get; set; }
    }

    public class UpdateSystemMenuRequest
    {
        [Required]
        public long id { get; set; }

        public long? parent_id { get; set; }

        [Required, StringLength(100)]
        public string name { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string code { get; set; } = string.Empty;

        public string? icon { get; set; }

        [StringLength(200)]
        public string? path { get; set; }

        public int? order { get; set; }

        public bool? is_visible { get; set; }

        [StringLength(100)]
        public string? permission { get; set; }
    }

    public class AssignMenusRequest
    {
        [Required]
        public List<long> menu_ids { get; set; } = new();
    }
}
