using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProtoHub.Api.DTOs
{
    public class MenuGroupListResponse
    {
        public long id { get; set; }
        public string name { get; set; } = string.Empty;
        public string? icon { get; set; }
        public int order { get; set; }
        public string? description { get; set; }
        public System.DateTime create_time { get; set; }
        public System.DateTime update_time { get; set; }
    }

    public class CreateMenuGroupRequest
    {
        [Required, StringLength(100)]
        public string name { get; set; } = string.Empty;

        public int order { get; set; }

        [StringLength(500)]
        public string? description { get; set; }

        public List<ProjectMemberInput>? members { get; set; }
    }

    public class ProjectMemberInput
    {
        [Required]
        public long user_id { get; set; }

        [Required, StringLength(50)]
        public string access_type { get; set; } = "view";
    }

    public class UpdateMenuGroupRequest
    {
        [Required]
        public long id { get; set; }

        [Required, StringLength(100)]
        public string name { get; set; } = string.Empty;

        public int? order { get; set; }

        [StringLength(500)]
        public string? description { get; set; }
    }
}
