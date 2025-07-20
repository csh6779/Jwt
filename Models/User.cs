using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JwtApi.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(15)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty; // 비밀번호는 해시로 저장

        [Required]
        public string Role { get; set; } = "User"; // 기본 권한

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
