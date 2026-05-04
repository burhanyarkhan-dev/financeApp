using FinanceApp.Data;
using FinanceApp.DTOs;
using FinanceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Services
{
    public interface IUserService
    {
        Task<bool> PhoneExistsAsync(string phoneNumber);
        Task<UserProfileResponse?> GetByPhoneAsync(string phoneNumber);
        Task<(UserProfileResponse? Profile, bool AlreadyExists)> CreatePendingUserAsync(string phoneNumber, bool isMigrated = false);
        Task<UserProfileResponse?> SavePersonalInfoAsync(PersonalInfoRequest request);
        Task<bool> AcceptPrivacyPolicyAsync(string phoneNumber);
        Task<bool> SetPinAsync(string phoneNumber, string pin);
        Task<bool> ChangePinAsync(string phoneNumber, string currentPin, string newPin);
        Task<bool> ValidatePinAsync(string phoneNumber, string pin);
        Task<UserProfileResponse?> UpdateProfileAsync(UpdateProfileRequest request);
        Task<bool> SetBiometricAsync(string phoneNumber, bool enable);
    }

    public class UserService : IUserService
    {
        private readonly AppDbContext _db;

        public UserService(AppDbContext db) => _db = db;

        public Task<bool> PhoneExistsAsync(string phone) =>
            _db.Users.AnyAsync(u => u.PhoneNumber == phone);

        public async Task<UserProfileResponse?> GetByPhoneAsync(string phone)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone);
            return user is null ? null : Map(user);
        }

        public async Task<(UserProfileResponse? Profile, bool AlreadyExists)> CreatePendingUserAsync(string phone, bool isMigrated = false)
        {
            var existing = await _db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone);
            if (existing is not null)
                return (null, true); // phone already registered — caller returns 409

            var user = new User
            {
                PhoneNumber = phone,
                IsMigratedUser = isMigrated,
                CurrentStep = RegistrationStep.OtpVerified
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return (Map(user), false);
        }

        public async Task<UserProfileResponse?> SavePersonalInfoAsync(PersonalInfoRequest req)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == req.PhoneNumber);
            if (user is null) return null;

            user.FirstName = req.FirstName;
            user.LastName = req.LastName;
            user.Email = req.Email;
            user.DateOfBirth = req.DateOfBirth;
            user.NationalId = req.NationalId;
            user.CurrentStep = RegistrationStep.PersonalInfoSaved;

            await _db.SaveChangesAsync();
            return Map(user);
        }

        public async Task<bool> AcceptPrivacyPolicyAsync(string phone)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone);
            if (user is null) return false;

            user.HasAcceptedPrivacyPolicy = true;
            user.CurrentStep = RegistrationStep.PrivacyPolicyAccepted;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetPinAsync(string phone, string pin)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone);
            if (user is null) return false;

            user.Pin = pin;
            user.CurrentStep = RegistrationStep.Completed;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangePinAsync(string phone, string currentPin, string newPin)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone);
            if (user is null || user.Pin != currentPin) return false;

            user.Pin = newPin;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ValidatePinAsync(string phone, string pin)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone);
            return user is not null && user.Pin == pin;
        }

        public async Task<UserProfileResponse?> UpdateProfileAsync(UpdateProfileRequest req)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == req.PhoneNumber);
            if (user is null) return null;

            if (req.FirstName is not null) user.FirstName = req.FirstName;
            if (req.LastName is not null) user.LastName = req.LastName;
            if (req.Email is not null) user.Email = req.Email;
            if (req.ProfileImageUrl is not null) user.ProfileImageUrl = req.ProfileImageUrl;

            await _db.SaveChangesAsync();
            return Map(user);
        }

        public async Task<bool> SetBiometricAsync(string phone, bool enable)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone);
            if (user is null) return false;

            user.IsBiometricEnabled = enable;
            await _db.SaveChangesAsync();
            return true;
        }

        private static UserProfileResponse Map(User u) => new()
        {
            Id = u.Id,
            FirstName = u.FirstName,
            LastName = u.LastName,
            PhoneNumber = u.PhoneNumber,
            Email = u.Email,
            IsBiometricEnabled = u.IsBiometricEnabled,
            HasAcceptedPrivacyPolicy = u.HasAcceptedPrivacyPolicy,
            IsMigratedUser = u.IsMigratedUser,
            RegistrationStep = u.CurrentStep.ToString(),
            ProfileImageUrl = u.ProfileImageUrl,
            CreatedAt = u.CreatedAt
        };
    }
}
