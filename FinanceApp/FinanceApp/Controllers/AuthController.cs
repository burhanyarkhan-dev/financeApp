using FinanceApp.DTOs;
using FinanceApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.Controllers
{
    /// <summary>
    /// PIN-based login and PIN management. No JWT — returns the user profile on success.
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _users;

        public AuthController(IUserService users) => _users = users;

        /// <summary>
        /// Authenticate using phone number + PIN.
        /// Returns the user profile on success.
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), 401)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var valid = await _users.ValidatePinAsync(request.PhoneNumber, request.Pin);
            if (!valid)
                return Unauthorized(ApiResponse<UserProfileResponse>.Fail("Invalid phone number or PIN."));

            var profile = await _users.GetByPhoneAsync(request.PhoneNumber);
            return Ok(ApiResponse<UserProfileResponse>.Ok(profile!, "Login successful."));
        }

        /// <summary>
        /// Change PIN. Requires the current PIN to be supplied.
        /// </summary>
        [HttpPost("change-pin")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> ChangePin([FromBody] ChangePinRequest request)
        {
            var ok = await _users.ChangePinAsync(request.PhoneNumber, request.CurrentPin, request.NewPin);
            if (!ok)
                return BadRequest(ApiResponse<object>.Fail("Current PIN is incorrect or user not found."));

            return Ok(ApiResponse<object>.Ok(new { }, "PIN changed successfully."));
        }

        /// <summary>
        /// Toggle biometric authentication for the given phone number.
        /// </summary>
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
