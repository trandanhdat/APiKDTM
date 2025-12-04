using System.ComponentModel.DataAnnotations;

namespace APi.Models.DTO
{
    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}
