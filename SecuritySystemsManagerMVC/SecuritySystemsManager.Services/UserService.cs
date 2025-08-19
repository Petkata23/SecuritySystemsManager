using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using SecuritySystemsManager.Data;
using SecuritySystemsManager.Data.Entities;
using SecuritySystemsManager.Shared.Attributes;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Security;
using SecuritySystemsManager.Shared.Services.Contracts;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;

namespace SecuritySystemsManager.Services
{
    [AutoBind]
    public class UserService : BaseCrudService<UserDto, IUserRepository>, IUserService
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly UserManager<User> _userManager;
        private readonly SecuritySystemsManagerDbContext _dbContext;
        private readonly UrlEncoder _urlEncoder;

        public UserService(
            IUserRepository repository,
            IFileStorageService fileStorageService,
            UserManager<User> userManager,
            SecuritySystemsManagerDbContext dbContext,
            UrlEncoder? urlEncoder = null) : base(repository)
        {
            _fileStorageService = fileStorageService;
            _userManager = userManager;
            _dbContext = dbContext;
            _urlEncoder = urlEncoder ?? UrlEncoder.Default;
        }

        public override async Task DeleteAsync(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            // Check for dependencies that block deletion (excluding chat messages)
            int ordersAsClientCount = await _dbContext.Orders.CountAsync(o => o.ClientId == id);
            int installedDevicesCount = await _dbContext.InstalledDevices.CountAsync(d => d.InstalledById == id);
            int maintenanceLogsCount = await _dbContext.MaintenanceLogs.CountAsync(m => m.TechnicianId == id);

            if (ordersAsClientCount > 0 || installedDevicesCount > 0 || maintenanceLogsCount > 0)
            {
                var reasons = new List<string>();
                if (ordersAsClientCount > 0) reasons.Add($"has {ordersAsClientCount} orders as client");
                if (installedDevicesCount > 0) reasons.Add($"has {installedDevicesCount} installations as technician");
                if (maintenanceLogsCount > 0) reasons.Add($"has {maintenanceLogsCount} maintenance logs as technician");

                string message = "User cannot be deleted: " + string.Join(", ", reasons) + ". Please remove or reassign these dependencies first.";
                throw new InvalidOperationException(message);
            }

            // 1) Delete chat messages (sent and received)
            var sentMessages = _dbContext.ChatMessages.Where(m => m.SenderId == id);
            var receivedMessages = _dbContext.ChatMessages.Where(m => m.RecipientId == id);
            _dbContext.ChatMessages.RemoveRange(sentMessages);
            _dbContext.ChatMessages.RemoveRange(receivedMessages);

            // 2) Remove roles/logins/claims via UserManager to avoid FK issues
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, roles);
            }

            // 3) Persist message deletions before deleting the user
            await _dbContext.SaveChangesAsync();

            // 4) Delete the user via UserManager (cleans up AspNet* dependencies)
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to delete user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        public async Task<bool> CanUserLoginAsync(string username, string password)
        {
            var user = await _repository.GetByUsernameAsync(username);

            if (user == null)
            {
                return false;
            }

            // If we have UserManager, use it for password validation
            var userEntity = await _userManager.FindByNameAsync(username);
            return userEntity != null && await _userManager.CheckPasswordAsync(userEntity, password);

            // Fall back to the legacy password verification
            return PasswordHasher.VerifyPassword(password, user.Password);
        }

        public Task<UserDto> GetByUsernameAsync(string username)
        {
            return _repository.GetByUsernameAsync(username);
        }
        
        public async Task<UserDto> CreateUserWithPasswordAsync(UserDto userDto, string password)
        {
            // If we have UserManager, use it for password hashing
            if (_userManager != null)
            {
                var user = new User
                {
                    Id = userDto.Id, // Set the expected ID
                    UserName = userDto.Username,
                    Email = userDto.Email,
                    FirstName = userDto.FirstName,
                    LastName = userDto.LastName,
                    ProfileImage = userDto.ProfileImage,
                    RoleId = userDto.RoleId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                
                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    // Add user to the appropriate role based on RoleId
                    string roleName = ((SecuritySystemsManager.Shared.Enums.RoleType)userDto.RoleId).ToString();
                    await _userManager.AddToRoleAsync(user, roleName);
                    
                    return await _repository.GetByIdAsync(user.Id);
                }
                throw new InvalidOperationException($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
            
            // Fall back to legacy password hashing
            userDto.Password = PasswordHasher.HashPassword(password);
            await SaveAsync(userDto);
            return userDto;
        }
        
        public async Task<UserDto> UpdateUserWithPasswordAsync(UserDto userDto, string? password = null)
        {
            // If we have UserManager and password is provided, use it for password hashing
            {
                var user = await _userManager.FindByIdAsync(userDto.Id.ToString());
                if (user != null)
                {
                    // Check if the role has changed
                    int oldRoleId = user.RoleId ?? 0;
                    
                    user.Email = userDto.Email;
                    user.FirstName = userDto.FirstName;
                    user.LastName = userDto.LastName;
                    user.ProfileImage = userDto.ProfileImage;
                    user.RoleId = userDto.RoleId;
                    user.UpdatedAt = DateTime.Now;
                    
                    var result = await _userManager.UpdateAsync(user);
                    if (!result.Succeeded)
                    {
                        throw new InvalidOperationException($"Failed to update user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                    
                    // Update role if changed
                    if (oldRoleId != userDto.RoleId)
                    {
                        // Get current roles
                        var currentRoles = await _userManager.GetRolesAsync(user);
                        
                        // Remove from all current roles
                        if (currentRoles.Any())
                        {
                            await _userManager.RemoveFromRolesAsync(user, currentRoles);
                        }
                        
                        // Add to new role
                        string newRoleName = ((SecuritySystemsManager.Shared.Enums.RoleType)userDto.RoleId).ToString();
                        await _userManager.AddToRoleAsync(user, newRoleName);
                    }
                    
                    if (!string.IsNullOrEmpty(password))
                    {
                        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                        result = await _userManager.ResetPasswordAsync(user, token, password);
                        if (!result.Succeeded)
                        {
                            throw new InvalidOperationException($"Failed to update password: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                        }
                    }
                    
                    return await _repository.GetByIdAsync(user.Id);
                }
            }
            
            // Fall back to legacy approach
            if (!string.IsNullOrEmpty(password))
            {
                userDto.Password = PasswordHasher.HashPassword(password);
            }
            else
            {
                // Get the existing user to preserve the password
                var existingUser = await _repository.GetByIdAsync(userDto.Id);
                if (existingUser != null)
                {
                    userDto.Password = existingUser.Password;
                }
            }
            
            await SaveAsync(userDto);
            return userDto;
        }
        
        public async Task<string> UploadUserProfileImageAsync(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return null;

            // Use the file storage service to upload the file
            return await _fileStorageService.UploadFileAsync(imageFile, "uploads/profiles");
        }
        
        public async Task<string> UploadProfileImageAsync(int userId, IFormFile profileImageFile)
        {
            // Upload the image first
            var imagePath = await UploadUserProfileImageAsync(profileImageFile);
            
            if (!string.IsNullOrEmpty(imagePath))
            {
                // Get the user and update their profile image
                var user = await _repository.GetByIdAsync(userId);
                if (user != null)
                {
                    user.ProfileImage = imagePath;
                    await _repository.SaveAsync(user);
                }
            }
            
            return imagePath;
        }
        
        public async Task<UserDto> CreateUserWithDetailsAsync(UserDto userDto, string password, IFormFile profileImageFile)
        {
            // Validate password
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password is required");
            }
            
            // Upload profile image if provided
            if (profileImageFile != null && profileImageFile.Length > 0)
            {
                userDto.ProfileImage = await UploadUserProfileImageAsync(profileImageFile);
            }
            
            // Create user with password
            return await CreateUserWithPasswordAsync(userDto, password);
        }

        public async Task<UserProfileDto> GetUserProfileDataAsync(int userId)
        {
            var user = await _repository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            // Get user role
            string userRole = "User";
            if (_userManager != null)
            {
                var userEntity = await _userManager.FindByIdAsync(userId.ToString());
                if (userEntity != null)
                {
                    var userRoles = await _userManager.GetRolesAsync(userEntity);
                    userRole = userRoles.FirstOrDefault() ?? "User";
                }
            }

            // Get user statistics
            var totalOrders = user.OrdersAsClient?.Count ?? 0;
            var totalLocations = user.OrdersAsClient?.Select(o => o.LocationId).Distinct().Count() ?? 0;

            return new UserProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                PhoneNumber = "", // UserDto doesn't have PhoneNumber
                FirstName = user.FirstName,
                LastName = user.LastName,
                ProfileImage = user.ProfileImage ?? "",
                TwoFactorEnabled = false, // Would need to be calculated
                HasAuthenticator = false, // Would need to be calculated
                RecoveryCodesLeft = 0, // Would need to be calculated
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                LastLoginTime = null, // IdentityUser doesn't have LastLoginTime by default
                TotalOrders = totalOrders,
                TotalLocations = totalLocations,
                UserRole = userRole
            };
        }

        // New methods for account management business logic
        public async Task<(bool Success, List<string> Errors)> UpdateUserProfileAsync(int userId, string email, string phoneNumber, string firstName, string lastName, IFormFile profileImageFile)
        {
            var errors = new List<string>();
            
            if (_userManager == null)
            {
                errors.Add("User manager is not available");
                return (false, errors);
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                errors.Add("User not found");
                return (false, errors);
            }

            // Update email if changed
            var currentEmail = await _userManager.GetEmailAsync(user);
            if (email != currentEmail)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, email);
                if (!setEmailResult.Succeeded)
                {
                    errors.AddRange(setEmailResult.Errors.Select(e => e.Description));
                }
            }

            // Update phone number if changed
            var currentPhoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (phoneNumber != currentPhoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, phoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    errors.AddRange(setPhoneResult.Errors.Select(e => e.Description));
                }
            }

            // Update additional fields
            user.FirstName = firstName;
            user.LastName = lastName;

            // Handle profile image
            if (profileImageFile != null && profileImageFile.Length > 0)
            {
                string imageUrl = await UploadUserProfileImageAsync(profileImageFile);
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    user.ProfileImage = imageUrl;
                }
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                errors.AddRange(updateResult.Errors.Select(e => e.Description));
            }

            return (errors.Count == 0, errors);
        }

        public async Task<(bool Success, List<string> Errors)> ChangeUserPasswordAsync(int userId, string oldPassword, string newPassword)
        {
            var errors = new List<string>();
            
            if (_userManager == null)
            {
                errors.Add("User manager is not available");
                return (false, errors);
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                errors.Add("User not found");
                return (false, errors);
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
            if (!changePasswordResult.Succeeded)
            {
                errors.AddRange(changePasswordResult.Errors.Select(e => e.Description));
            }

            return (errors.Count == 0, errors);
        }

        public async Task<(bool Success, List<string> Errors)> SetUserPasswordAsync(int userId, string newPassword)
        {
            var errors = new List<string>();
            
            if (_userManager == null)
            {
                errors.Add("User manager is not available");
                return (false, errors);
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                errors.Add("User not found");
                return (false, errors);
            }

            var addPasswordResult = await _userManager.AddPasswordAsync(user, newPassword);
            if (!addPasswordResult.Succeeded)
            {
                errors.AddRange(addPasswordResult.Errors.Select(e => e.Description));
            }

            return (errors.Count == 0, errors);
        }

        public async Task<bool> UserHasPasswordAsync(int userId)
        {
            if (_userManager == null)
                return false;

            var user = await _userManager.FindByIdAsync(userId.ToString());
            return user != null && await _userManager.HasPasswordAsync(user);
        }

        public async Task<(bool HasAuthenticator, bool Is2faEnabled, int RecoveryCodesLeft)> GetTwoFactorAuthInfoAsync(int userId)
        {
            if (_userManager == null)
                return (false, false, 0);

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return (false, false, 0);

            return (
                await _userManager.GetAuthenticatorKeyAsync(user) != null,
                await _userManager.GetTwoFactorEnabledAsync(user),
                await _userManager.CountRecoveryCodesAsync(user)
            );
        }

        public async Task<(string SharedKey, string AuthenticatorUri)> GetTwoFactorSetupInfoAsync(int userId)
        {
            if (_userManager == null)
                return (string.Empty, string.Empty);

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return (string.Empty, string.Empty);

            var authenticatorKey = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(authenticatorKey))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                authenticatorKey = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            return (
                await FormatAuthenticatorKey(authenticatorKey),
                await GenerateQrCodeUriAsync(user.Email, authenticatorKey)
            );
        }

        public async Task<(bool Success, List<string> Errors, string[] RecoveryCodes)> EnableTwoFactorAuthAsync(int userId, string verificationCode)
        {
            var errors = new List<string>();
            string[] recoveryCodes = new string[0];
            
            if (_userManager == null)
            {
                errors.Add("User manager is not available");
                return (false, errors, recoveryCodes);
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                errors.Add("User not found");
                return (false, errors, recoveryCodes);
            }

            if (string.IsNullOrEmpty(verificationCode))
            {
                errors.Add("Verification code is required");
                return (false, errors, recoveryCodes);
            }

            // Strip spaces and hyphens
            var cleanCode = verificationCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
                user, _userManager.Options.Tokens.AuthenticatorTokenProvider, cleanCode);

            if (!is2faTokenValid)
            {
                errors.Add("Verification code is invalid");
                return (false, errors, recoveryCodes);
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);
            
            // Generate recovery codes
            recoveryCodes = (await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10)).ToArray();

            return (true, errors, recoveryCodes);
        }

        public async Task<(bool Success, List<string> Errors)> DisableTwoFactorAuthAsync(int userId)
        {
            var errors = new List<string>();
            
            if (_userManager == null)
            {
                errors.Add("User manager is not available");
                return (false, errors);
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                errors.Add("User not found");
                return (false, errors);
            }

            if (!await _userManager.GetTwoFactorEnabledAsync(user))
            {
                errors.Add("Two-factor authentication is not enabled");
                return (false, errors);
            }

            await _userManager.SetTwoFactorEnabledAsync(user, false);
            return (true, errors);
        }

        public async Task<(bool Success, List<string> Errors)> ResetAuthenticatorAsync(int userId)
        {
            var errors = new List<string>();
            
            if (_userManager == null)
            {
                errors.Add("User manager is not available");
                return (false, errors);
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                errors.Add("User not found");
                return (false, errors);
            }

            await _userManager.SetTwoFactorEnabledAsync(user, false);
            await _userManager.ResetAuthenticatorKeyAsync(user);
            await _userManager.UpdateAsync(user);

            return (true, errors);
        }

        public async Task<string> FormatAuthenticatorKey(string unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }

        public async Task<string> GenerateQrCodeUriAsync(string email, string unformattedKey)
        {
            return string.Format(
                "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6",
                _urlEncoder.Encode("Security Systems Manager"),
                _urlEncoder.Encode(email),
                unformattedKey);
        }
        
        public async Task<IEnumerable<UserDto>> GetUsersGroupedByRoleAsync()
        {
            return await _repository.GetAllGroupedByRoleAsync();
        }
    }
} 