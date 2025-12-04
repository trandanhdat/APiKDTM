using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APi.Models.Role
{
    [Table("UserRoleHistories")]
    public class UserRoleHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public string? PreviousRole { get; set; }

        [Required]
        public string? NewRole { get; set; }

        public string? Reason { get; set; }

        [Required]
        public int ChangedByUserId { get; set; }

        [Required]
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public virtual UserModel? User { get; set; }
        [ForeignKey(nameof(ChangedByUserId))]
        public virtual UserModel? ChangedByUser { get; set; }
    }
}
