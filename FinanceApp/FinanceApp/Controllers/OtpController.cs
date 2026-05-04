using FinanceApp.DTOs;
using FinanceApp.Models;
using FinanceApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.Controllers
{
    /// <summary>
    /// Send and verify OTPs for all flows.
    /// </summary>
    [ApiController]
    [Route("api/otp")]
    [Produces("application/json")]
    public class OtpController : ControllerBase
    {
        private readonly IOtpService _otp;

        public OtpController(IOtpService otp) => _otp = otp;

        /// <summary>
        /// Send a 6-digit OTP to the given phone number.
        /// Purpose values: registration | migration | login | pin_reset
        /// </summary>
        [HttpPost("send")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> Send([FromBody] SendOtpRequest request)
        {
            if (!Enum.TryParse<OtpPurpose>(request.Purpose, ignoreCase: true, out var purpose))
                return BadRequest(ApiResponse<object>.Fail(
                    "Invalid purpose. Allowed: registration, migration, login, pin_reset"));

            var code = await _otp.SendOtpAsync(request.PhoneNumber, purpose);

            // Return the code in the response body (dev/testing convenience — remove in production)
            return Ok(ApiResponse<object>.Ok(
                new { SentTo = MaskPhone(request.PhoneNumber), Code = code },
                "OTP sent successfully."));
        }

        /// <summary>
        /// Verify the OTP code. Returns verified=true on success.
        /// </summary>
        [HttpPost("verify")]
        [ProducesResponseType(typeof(ApiResponse<OtpResult>), 200)]
        [ProducesResponseType(typeof(ApiResponse<OtpResult>), 400)]
        public async Task<IActionResult> Verify([FromBody] VerifyOtpRequest request)
        {
            if (!Enum.TryParse<OtpPurpose>(request.Purpose, ignoreCase: true, out var purpose))
                return BadRequest(ApiResponse<OtpResult>.Fail("Invalid purpose."));

            var verified = await _otp.VerifyOtpAsync(request.PhoneNumber, request.Code, purpose);

            if (!verified)
                return BadRequest(ApiResponse<OtpResult>.Ok(
                    new OtpResult { Verified = false, Message = "Invalid or expired OTP." }));

            return Ok(ApiResponse<OtpResult>.Ok(
                new OtpResult { Verified = true, Message = "OTP verified successfully." }));
        }

        private static string MaskPhone(string p) =>
            p.Length >= 4 ? $"****{p[^4..]}" : "****";
    }
}
