using APi.Models.Role;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APi.Models
{
    [Table("Users")]
    public class UserModel

    {
        [Key]
        public int id { get; set; }

        [Required]
        [MaxLength(50)]
        public string? username { get; set; }

        [Required]
        public string? password { get; set; }

        [Required]
        [EmailAddress]
        public string? email { get; set; }

        [MaxLength(11)]
        public string? phone { get; set; }

        public string? fullname { get; set; }

        public string? role { get; set; } = "User";

        public bool isActive { get; set; } = true;

        public DateTime createdAt { get; set; } = DateTime.UtcNow;

        public DateTime? lastLogin { get; set; }
        public virtual ICollection<UserRoleHistory>? RoleHistories {  get; set; }
        public virtual ICollection<UserRoleHistory>? ChangedRoleHistories {  get; set; }

    }
}
