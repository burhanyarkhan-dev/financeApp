using FinanceApp.Data;
using FinanceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Services
{
    public interface IOtpService
    {
        Task<string> SendOtpAsync(string phoneNumber, OtpPurpose purpose);
        Task<bool> VerifyOtpAsync(string phoneNumber, string code, OtpPurpose purpose);
    }

    public class OtpService : IOtpService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<OtpService> _logger;

        private const int ExpiryMinutes = 5;
        private const int MaxAttempts = 3;

        public OtpService(AppDbContext db, ILogger<OtpService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<string> SendOtpAsync(string phoneNumber, OtpPurpose purpose)
        {
            // Expire any existing active OTPs for this phone + purpose
            var active = await _db.OtpCodes
                .Where(o => o.PhoneNumber == phoneNumber
                         && o.Purpose == purpose
                         && !o.IsUsed
                         && o.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();

            active.ForEach(o => o.IsUsed = true);

            // Generate plain 6-digit code (no hashing — no third-party dependency)
            var code = new Random().Next(100000, 999999).ToString();

            _db.OtpCodes.Add(new OtpCode
            {
                PhoneNumber = phoneNumber,
                Code = code,
                Purpose = purpose,
                ExpiresAt = DateTime.UtcNow.AddMinutes(ExpiryMinutes)
            });

            await _db.SaveChangesAsync();

            // Log only — plug in any SMS provider here later (no dependency added)
            _logger.LogInformation("[OTP] Phone={Phone} Code={Code} Purpose={Purpose} ExpiresAt={Exp}",
                phoneNumber, code, purpose, DateTime.UtcNow.AddMinutes(ExpiryMinutes));

            return code;
        }

        public async Task<bool> VerifyOtpAsync(string phoneNumber, string code, OtpPurpose purpose)
        {
            var otp = await _db.OtpCodes
                .Where(o => o.PhoneNumber == phoneNumber
                         && o.Purpose == purpose
                         && !o.IsUsed
                         && o.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (otp is null) return false;

            otp.AttemptCount++;

            if (otp.AttemptCount > MaxAttempts)
            {
                otp.IsUsed = true;
                await _db.SaveChangesAsync();
                return false;
            }

            if (otp.Code != code)
            {
                await _db.SaveChangesAsync();
                return false;
            }

            otp.IsUsed = true;
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
