using FinanceApp.DTOs;
using FinanceApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.Controllers
{
    /// <summary>
    /// Migrate Existing User flow.
    ///
    /// Order of calls:
    ///   1. POST /api/otp/send             (purpose=migration)
    ///   2. POST /api/otp/verify           (purpose=migration)
    ///   3. POST /api/migration/init       — creates / locates user record
    ///   4. POST /api/migration/accept-privacy-policy
    ///   5. POST /api/migration/set-pin
    ///   6. POST /api/migration/biometrics (optional)
    /// </summary>
    [ApiController]
    [Route("api/migration")]
    [Produces("application/json")]
    public class MigrationController : ControllerBase
    {
        private readonly IUserService _users;

        public MigrationController(IUserService users) => _users = users;

        // ── Step 3 ────────────────────────────────────────────────────────────

        /// <summary>
        /// Create or locate the user record after OTP verification.
        /// Marks the user as a migrated user.
        /// </summary>
        [HttpPost("init")]
        [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), 409)]
        public async Task<IActionResult> Init([FromBody] SendOtpRequest request)
        {
            var (profile, alreadyExists) = await _users.CreatePendingUserAsync(request.PhoneNumber, isMigrated: true);

            if (alreadyExists)
                return Conflict(ApiResponse<UserProfileResponse>.Fail(
                    $"A user with phone number {request.PhoneNumber} is already registered."));

            return Ok(ApiResponse<UserProfileResponse>.Ok(profile!, "User ready for migration."));
        }

        // ── Step 4 ────────────────────────────────────────────────────────────

        /// <summary>Accept the privacy policy for the migrated user.</summary>
        [HttpPost("accept-privacy-policy")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> AcceptPolicy([FromBody] AcceptPolicyRequest request)
        {
            if (!request.Accepted)
                return BadRequest(ApiResponse<object>.Fail("Privacy policy must be accepted."));

            var ok = await _users.AcceptPrivacyPolicyAsync(request.PhoneNumber);
            if (!ok) return NotFound(ApiResponse<object>.Fail("User not found."));

            return Ok(ApiResponse<object>.Ok(new { }, "Privacy policy accepted."));
        }

        // ── Step 5 ────────────────────────────────────────────────────────────

        /// <summary>Set PIN to complete migration.</summary>
        [HttpPost("set-pin")]
        [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), 404)]
        public async Task<IActionResult> SetPin([FromBody] SetPinRequest request)
        {
            var ok = await _users.SetPinAsync(request.PhoneNumber, request.Pin);
            if (!ok) return NotFound(ApiResponse<UserProfileResponse>.Fail("User not found."));

            var profile = await _users.GetByPhoneAsync(request.PhoneNumber);
            return Ok(ApiResponse<UserProfileResponse>.Ok(profile!, "Migration complete."));
        }

        // ── Step 6 (Optional) ─────────────────────────────────────────────────

        /// <summary>Enable or disable biometric authentication.</summary>
        [HttpPost("biometrics")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> Biometrics([FromBody] BiometricRequest request)
        {
            var ok = await _users.SetBiometricAsync(request.PhoneNumber, request.Enable);
            if (!ok) return NotFound(ApiResponse<object>.Fail("User not found."));

            return Ok(ApiResponse<object>.Ok(
                new { IsBiometricEnabled = request.Enable },
                request.Enable ? "Biometrics enabled." : "Biometrics disabled."));
        }
    }
}
