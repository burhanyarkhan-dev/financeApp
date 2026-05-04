using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Models
{
    public class OtpCode
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required, MaxLength(6)]
        public string Code { get; set; } = string.Empty;

        public OtpPurpose Purpose { get; set; }

        public bool IsUsed { get; set; } = false;

        public int AttemptCount { get; set; } = 0;

        public DateTime ExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid? UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
    }

    public enum OtpPurpose
    {
        Registration = 1,
        Migration = 2,
        Login = 3,
        PinReset = 4
    }
}
