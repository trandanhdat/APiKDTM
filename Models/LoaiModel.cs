using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APi.Models
{
    [Table("Loai")]
    public class LoaiModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string MaLoai { get; set; }

        public string TenLoai { get; set; }

        public string MoTa { get; set; }
    }
}
