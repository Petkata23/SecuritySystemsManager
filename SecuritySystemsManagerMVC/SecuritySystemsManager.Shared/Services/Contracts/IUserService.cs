using Microsoft.AspNetCore.Http;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;

namespace SecuritySystemsManager.Shared.Services.Contracts
{
    public interface IUserService : IBaseCrudService<UserDto, IUserRepository>
    {
        Task<bool> CanUserLoginAsync(string username, string password);
        Task<UserDto> GetByUsernameAsync(string username);
        Task<UserDto> CreateUserWithPasswordAsync(UserDto userDto, string password);
        Task<UserDto> UpdateUserWithPasswordAsync(UserDto userDto, string? password = null);
        Task<string> UploadUserProfileImageAsync(IFormFile imageFile);
        Task<string> UploadProfileImageAsync(int userId, IFormFile profileImageFile);
        Task<UserDto> CreateUserWithDetailsAsync(UserDto userDto, string password, IFormFile profileImageFile);
        Task<UserProfileDto> GetUserProfileDataAsync(int userId);
        
        // New methods for account management business logic
        Task<(bool Success, List<string> Errors)> UpdateUserProfileAsync(int userId, string email, string phoneNumber, string firstName, string lastName, IFormFile profileImageFile);
        Task<(bool Success, List<string> Errors)> ChangeUserPasswordAsync(int userId, string oldPassword, string newPassword);
        Task<(bool Success, List<string> Errors)> SetUserPasswordAsync(int userId, string newPassword);
        Task<bool> UserHasPasswordAsync(int userId);
        Task<(bool HasAuthenticator, bool Is2faEnabled, int RecoveryCodesLeft)> GetTwoFactorAuthInfoAsync(int userId);
        Task<(string SharedKey, string AuthenticatorUri)> GetTwoFactorSetupInfoAsync(int userId);
        Task<(bool Success, List<string> Errors, string[] RecoveryCodes)> EnableTwoFactorAuthAsync(int userId, string verificationCode);
        Task<(bool Success, List<string> Errors)> DisableTwoFactorAuthAsync(int userId);
        Task<(bool Success, List<string> Errors)> ResetAuthenticatorAsync(int userId);
        Task<string> FormatAuthenticatorKey(string unformattedKey);
        Task<string> GenerateQrCodeUriAsync(string email, string unformattedKey);
    }
} 