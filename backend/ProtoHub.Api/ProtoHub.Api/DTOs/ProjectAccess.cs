using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProtoHub.Api.DTOs
{
    public class ProjectAccessListRequest
    {
        public long? project_id { get; set; }
        public long? user_id { get; set; }
        public string? keyword { get; set; }
        [Range(1, int.MaxValue)]
        public int page { get; set; } = 1;
        [Range(1, 100)]
        public int page_size { get; set; } = 20;
    }

    public class ProjectAccessResponse
    {
        public long id { get; set; }
        public long user_id { get; set; }
        public string user_name { get; set; } = string.Empty;
        public string? nick_name { get; set; }
        public long project_id { get; set; }
        public string project_name { get; set; } = string.Empty;
        public string access_type { get; set; } = "view";
        public long? granted_by { get; set; }
        public string? granted_by_name { get; set; }
        public DateTime create_time { get; set; }
    }

    public class GrantAccessRequest
    {
        [Required]
        public long user_id { get; set; }

        [Required]
        public long project_id { get; set; }

        [Required, StringLength(50)]
        public string access_type { get; set; } = "view";
    }

    public class RevokeAccessRequest
    {
        [Required]
        public long id { get; set; }
    }

    public class BatchGrantAccessRequest
    {
        [Required]
        public List<long> user_ids { get; set; } = new();

        [Required]
        public long project_id { get; set; }

        [Required, StringLength(50)]
        public string access_type { get; set; } = "view";
    }

    public class UserProjectResponse
    {
        public long id { get; set; }
        public long project_id { get; set; }
        public string project_name { get; set; } = string.Empty;
        public string? project_icon { get; set; }
        public string access_type { get; set; } = "view";
        public DateTime create_time { get; set; }
    }

    public class UpdateAccessTypeRequest
    {
        [Required]
        public long id { get; set; }

        [Required, StringLength(50)]
        public string access_type { get; set; } = "view";
    }
}
