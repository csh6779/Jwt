#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JwtApi.Models
{
    [Table("user")]
    public class User
    {
        [Key]
        [Required]
        [Column("id")]
        public int Id { get; set; }

        [Column("username")]
        public string Name { get; set; } = String.Empty;

        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        [Column("password")]
        public string PasswordHash { get; set; } = string.Empty;

        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Column("phone")]
        public string Phone { get; set; } = string.Empty;

        [Column("role")]
        public string Role { get; set; } = "User";

        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
    }
}
