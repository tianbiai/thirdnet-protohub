using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProtoHub.Api.DTOs
{
    public class UserListRequest
    {
        public string? keyword { get; set; }
        public int? status { get; set; }
        [Range(1, int.MaxValue)]
        public int page { get; set; } = 1;
        [Range(1, 100)]
        public int page_size { get; set; } = 20;
    }

    public class UserListResponse
    {
        public long id { get; set; }
        public string user_name { get; set; } = string.Empty;
        public string? nick_name { get; set; }
        public string? email { get; set; }
        public int status { get; set; }
        public string? description { get; set; }
        public DateTime create_time { get; set; }
        public DateTime update_time { get; set; }
    }

    public class UserCreateRequest
    {
        [Required, StringLength(100)]
        public string user_name { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 6)]
        public string password { get; set; } = string.Empty;

        [StringLength(100)]
        public string? nick_name { get; set; }

        [StringLength(200)]
        public string? email { get; set; }

        public int? status { get; set; }

        public string? description { get; set; }
    }

    public class UserUpdateRequest
    {
        [Required]
        public long id { get; set; }

        [StringLength(100)]
        public string? nick_name { get; set; }

        [StringLength(200)]
        public string? email { get; set; }

        public int? status { get; set; }

        public string? description { get; set; }
    }

    public class AssignRolesRequest
    {
        [Required]
        public List<long> role_ids { get; set; } = new();
    }

    public class ResetPasswordRequest
    {
        [Required]
        public long id { get; set; }

        [Required, StringLength(100, MinimumLength = 6)]
        public string new_password { get; set; } = string.Empty;
    }

    public class RoleResponse
    {
        public long id { get; set; }
        public string code { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string? description { get; set; }
        public bool is_system { get; set; }
    }
}
