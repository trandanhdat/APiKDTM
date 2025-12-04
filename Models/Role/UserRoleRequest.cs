using Org.BouncyCastle.Utilities;
using System.ComponentModel.DataAnnotations;

namespace APi.Models.Role
{
    public class AssignRoleRequest
    {
        [Required]
        public int UserId {  get; set; }
        [Required]
        public string? Role {  get; set; }
        public string? Reason { get; set; }
    }
    public class BulkAssignRoleRequest
    {
        [Required]
        public List<int> UserId { get; set; }
        [Required]

        public string? Role { get; set; }
        public string? Reason { get; set; }
    }
    public class UserRoleResponse
    {
        public int UserId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? CurrentRole { get; set; }
        public string? PreviousRole { get; set; }
        public DateTime RoleChangedAt { get; set; }
        public string? ChangedBy { get; set; }
        public string? Reason { get; set; }

    }
}
