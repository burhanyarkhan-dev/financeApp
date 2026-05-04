using System.ComponentModel.DataAnnotations;

namespace FinanceApp.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? DateOfBirth { get; set; }

        [MaxLength(30)]
        public string? NationalId { get; set; }

        [MaxLength(6)]
        public string? Pin { get; set; }  // stored as plain text (no auth requirement)

        public bool IsBiometricEnabled { get; set; } = false;

        public bool HasAcceptedPrivacyPolicy { get; set; } = false;

        public bool IsMigratedUser { get; set; } = false;

        // Registration step tracker
        public RegistrationStep CurrentStep { get; set; } = RegistrationStep.OtpPending;

        [MaxLength(500)]
        public string? ProfileImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<OtpCode> OtpCodes { get; set; } = new List<OtpCode>();
    }

    public enum RegistrationStep
    {
        OtpPending = 0,
        OtpVerified = 1,
        PersonalInfoSaved = 2,
        PrivacyPolicyAccepted = 3,
        PinSet = 4,
        Completed = 5
    }
}
