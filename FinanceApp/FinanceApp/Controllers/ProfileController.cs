using FinanceApp.DTOs;
using FinanceApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.Controllers
{
    /// <summary>
    /// View and update the user profile.
    /// Pass the phone number in the request body — no auth headers needed.
    /// </summary>
    [ApiController]
    [Route("api/profile")]
    [Produces("application/json")]
    public class ProfileController : ControllerBase
    {
        private readonly IUserService _users;

        public ProfileController(IUserService users) => _users = users;

        /// <summary>
        /// Get user profile by phone number.
        /// </summary>
        [HttpGet("{phoneNumber}")]
        [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), 404)]
        public async Task<IActionResult> Get(string phoneNumber)
        {
            var profile = await _users.GetByPhoneAsync(phoneNumber);
            if (profile is null)
                return NotFound(ApiResponse<UserProfileResponse>.Fail("User not found."));

            return Ok(ApiResponse<UserProfileResponse>.Ok(profile));
        }

        /// <summary>
        /// Update profile fields (name, email, profile image).
        /// </summary>
        [HttpPatch]
        [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), 404)]
        public async Task<IActionResult> Update([FromBody] UpdateProfileRequest request)
        {
            var profile = await _users.UpdateProfileAsync(request);
            if (profile is null)
                return NotFound(ApiResponse<UserProfileResponse>.Fail("User not found."));

            return Ok(ApiResponse<UserProfileResponse>.Ok(profile, "Profile updated."));
        }
    }
}
