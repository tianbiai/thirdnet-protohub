using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ProtoHub.Database.Models;

namespace ProtoHub.Api.DTOs
{
    public class MenuItemListRequest
    {
        public long? group_id { get; set; }
    }

    public class MenuItemListResponse
    {
        public long id { get; set; }
        public long group_id { get; set; }
        public string name { get; set; } = string.Empty;
        public string type { get; set; } = "web";
        public string? url { get; set; }
        public string? description { get; set; }
        public int order { get; set; }
        public ViewportConfigModel? viewport_config { get; set; }
        public string? doc_file_id { get; set; }
        public string? doc_file_name { get; set; }
        public string? doc_description { get; set; }
        public string? route { get; set; }
        public DateTime create_time { get; set; }
        public DateTime update_time { get; set; }
    }

    public class CreateMenuItemRequest
    {
        public long? group_id { get; set; }

        [StringLength(100)]
        public string? name { get; set; }

        [StringLength(50)]
        public string? type { get; set; }

        [StringLength(500)]
        public string? url { get; set; }

        [StringLength(500)]
        public string? description { get; set; }

        public int? order { get; set; }

        public ViewportConfigModel? viewport_config { get; set; }
        public string? doc_file_id { get; set; }
        public string? doc_file_name { get; set; }
        public string? doc_description { get; set; }
        public string? route { get; set; }
    }

    public class UpdateMenuItemRequest
    {
        [Required]
        public long id { get; set; }

        public long? group_id { get; set; }

        [StringLength(100)]
        public string? name { get; set; }

        [StringLength(50)]
        public string? type { get; set; }

        [StringLength(500)]
        public string? url { get; set; }

        [StringLength(500)]
        public string? description { get; set; }

        public int? order { get; set; }

        public ViewportConfigModel? viewport_config { get; set; }
        public string? doc_file_id { get; set; }
        public string? doc_file_name { get; set; }
        public string? doc_description { get; set; }
        public string? route { get; set; }
    }
}
