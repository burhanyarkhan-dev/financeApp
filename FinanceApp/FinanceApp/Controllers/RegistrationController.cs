using FinanceApp.DTOs;
using FinanceApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.Controllers
{
    /// <summary>
    /// New Customer registration flow.
    ///
    /// Order of calls:
    ///   1. POST /check-phone
    ///   2. POST /api/otp/send        (purpose=registration)
    ///   3. POST /api/otp/verify      (purpose=registration)
    ///   4. POST /init                — creates a pending user record
    ///   5. POST /personal-info
    ///   6. POST /accept-privacy-policy
    ///   7. POST /set-pin             — completes registration
    /// </summary>
    [ApiController]
    [Route("api/registration")]
    [Produces("application/json")]
    public class RegistrationController : ControllerBase
    {
        private readonly IUserService _users;

        public RegistrationController(IUserService users) => _users = users;

        // ── Step 1 ────────────────────────────────────────────────────────────

        /// <summary>Check whether a phone number is already registered.</summary>
        [HttpPost("check-phone")]
        [ProducesResponseType(typeof(ApiResponse<CheckPhoneResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<CheckPhoneResponse>), 409)]
        public async Task<IActionResult> CheckPhone([FromBody] SendOtpRequest request)
        {
            var exists = await _users.PhoneExistsAsync(request.PhoneNumber);

            if (exists)
                return Conflict(ApiResponse<CheckPhoneResponse>.Ok(new CheckPhoneResponse
                {
                    PhoneExists = true,
                    Message = $"Phone number {request.PhoneNumber} is already registered. Please log in instead."
                }));

            return Ok(ApiResponse<CheckPhoneResponse>.Ok(new CheckPhoneResponse
            {
                PhoneExists = false,
                Message = "Phone number is available. You may proceed with registration."
            }));
        }

        // ── Step 4 ────────────────────────────────────────────────────────────

        /// <summary>
        /// Called right after OTP is verified. Creates a pending user row so
        /// subsequent steps have a record to update.
        /// </summary>
        [HttpPost("init")]
        [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), 409)]
        public async Task<IActionResult> Init([FromBody] SendOtpRequest request)
        {
            var (profile, alreadyExists) = await _users.CreatePendingUserAsync(request.PhoneNumber, isMigrated: false);

            if (alreadyExists)
                return Conflict(ApiResponse<UserProfileResponse>.Fail(
                    $"A user with phone number {request.PhoneNumber} is already registered."));

            return Ok(ApiResponse<UserProfileResponse>.Ok(profile!, "User initialised."));
        }

        // ── Step 5 ────────────────────────────────────────────────────────────

        /// <summary>Save the user's personal information.</summary>
        [HttpPost("personal-info")]
        [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), 404)]
        public async Task<IActionResult> PersonalInfo([FromBody] PersonalInfoRequest request)
        {
            var profile = await _users.SavePersonalInfoAsync(request);
            if (profile is null)
                return NotFound(ApiResponse<UserProfileResponse>.Fail(
                    "User not found. Call /api/registration/init first."));

            return Ok(ApiResponse<UserProfileResponse>.Ok(profile, "Personal info saved."));
        }

        // ── Step 6 ────────────────────────────────────────────────────────────

        /// <summary>Record the user's acceptance of the privacy policy.</summary>
        [HttpPost("accept-privacy-policy")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> AcceptPolicy([FromBody] AcceptPolicyRequest request)
        {
            if (!request.Accepted)
                return BadRequest(ApiResponse<object>.Fail(
                    "Privacy policy must be accepted to continue."));

            var ok = await _users.AcceptPrivacyPolicyAsync(request.PhoneNumber);
            if (!ok)
                return NotFound(ApiResponse<object>.Fail("User not found."));

            return Ok(ApiResponse<object>.Ok(new { }, "Privacy policy accepted."));
        }

        // ── Step 7 ────────────────────────────────────────────────────────────

        /// <summary>Set the user's PIN and complete registration.</summary>
        [HttpPost("set-pin")]
        [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), 400)]
        [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), 404)]
        public async Task<IActionResult> SetPin([FromBody] SetPinRequest request)
        {
            var ok = await _users.SetPinAsync(request.PhoneNumber, request.Pin);
            if (!ok)
                return NotFound(ApiResponse<UserProfileResponse>.Fail("User not found."));

            var profile = await _users.GetByPhoneAsync(request.PhoneNumber);
            return Ok(ApiResponse<UserProfileResponse>.Ok(profile!, "Registration complete."));
        }
    }
}
