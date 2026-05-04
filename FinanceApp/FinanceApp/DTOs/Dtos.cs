using System.ComponentModel.DataAnnotations;

namespace FinanceApp.DTOs
{
    // ── Common wrapper ─────────────────────────────────────────────────────────
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }

        public static ApiResponse<T> Ok(T data, string message = "Success") =>
            new() { Success = true, Message = message, Data = data };

        public static ApiResponse<T> Fail(string message, List<string>? errors = null) =>
            new() { Success = false, Message = message, Errors = errors };
    }

    // ── OTP ────────────────────────────────────────────────────────────────────
    public class SendOtpRequest
    {
        [Required, Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>registration | migration | login | pin_reset</summary>
        [Required]
        public string Purpose { get; set; } = string.Empty;
    }

    public class VerifyOtpRequest
    {
        [Required, Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required, StringLength(6, MinimumLength = 4)]
        public string Code { get; set; } = string.Empty;

        [Required]
        public string Purpose { get; set; } = string.Empty;
    }

    public class OtpResult
    {
        public bool Verified { get; set; }
        public string? Message { get; set; }
    }

    // ── Registration ───────────────────────────────────────────────────────────
    public class CheckPhoneResponse
    {
        public bool PhoneExists { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class PersonalInfoRequest
    {
        [Required, Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        public string? DateOfBirth { get; set; }

        public string? NationalId { get; set; }
    }

    public class AcceptPolicyRequest
    {
        [Required, Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        public bool Accepted { get; set; }
    }

    public class SetPinRequest
    {
        [Required, Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required, StringLength(6, MinimumLength = 4)]
        public string Pin { get; set; } = string.Empty;

        [Required, Compare(nameof(Pin), ErrorMessage = "PINs do not match")]
        public string ConfirmPin { get; set; } = string.Empty;
    }

    public class ChangePinRequest
    {
        [Required, Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required, StringLength(6, MinimumLength = 4)]
        public string CurrentPin { get; set; } = string.Empty;

        [Required, StringLength(6, MinimumLength = 4)]
        public string NewPin { get; set; } = string.Empty;

        [Required, Compare(nameof(NewPin))]
        public string ConfirmNewPin { get; set; } = string.Empty;
    }

    // ── Biometrics ─────────────────────────────────────────────────────────────
    public class BiometricRequest
    {
        [Required, Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        public bool Enable { get; set; }
    }

    // ── Login ──────────────────────────────────────────────────────────────────
    public class LoginRequest
    {
        [Required, Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required, StringLength(6, MinimumLength = 4)]
        public string Pin { get; set; } = string.Empty;
    }

    // ── Profile ────────────────────────────────────────────────────────────────
    public class UserProfileResponse
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public bool IsBiometricEnabled { get; set; }
        public bool HasAcceptedPrivacyPolicy { get; set; }
        public bool IsMigratedUser { get; set; }
        public string RegistrationStep { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateProfileRequest
    {
        [Required, Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? FirstName { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [MaxLength(500)]
        public string? ProfileImageUrl { get; set; }
    }
}
