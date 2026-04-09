using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProtoHub.Api.DTOs
{
    public class LoginRequest
    {
        [Required, StringLength(100)]
        public string user_name { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 6)]
        public string password { get; set; } = string.Empty;
    }

    public class ChangePasswordRequest
    {
        [Required, StringLength(100, MinimumLength = 6)]
        public string old_password { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 6)]
        public string new_password { get; set; } = string.Empty;
    }

    public class CurrentUserResponse
    {
        public long id { get; set; }
        public string user_name { get; set; } = string.Empty;
        public string? nick_name { get; set; }
        public string? email { get; set; }
        public int status { get; set; }
        public string? description { get; set; }
        public List<RoleItem> roles { get; set; } = new();
        public List<PermissionItem> permissions { get; set; } = new();
        public List<ProjectItem> projects { get; set; } = new();
    }

    public class RoleItem
    {
        public long id { get; set; }
        public string code { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
    }

    public class PermissionItem
    {
        public long id { get; set; }
        public string code { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string? category { get; set; }
    }

    public class ProjectItem
    {
        public long id { get; set; }
        public string name { get; set; } = string.Empty;
        public string access_type { get; set; } = string.Empty;
    }
}
