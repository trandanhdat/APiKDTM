using System.ComponentModel.DataAnnotations;

namespace APi.Models.DTO
{
    public class RegisterRequestDTO
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [MaxLength(11)]
        public string Phone { get; set; }

        public string Fullname { get; set; }
    }
}
