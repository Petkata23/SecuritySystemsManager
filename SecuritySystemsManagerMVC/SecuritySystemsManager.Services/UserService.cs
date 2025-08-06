using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using SecuritySystemsManager.Data.Entities;
using SecuritySystemsManager.Shared.Attributes;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Security;
using SecuritySystemsManager.Shared.Services.Contracts;
using System.IO;

namespace SecuritySystemsManager.Services
{
    [AutoBind]
    public class UserService : BaseCrudService<UserDto, IUserRepository>, IUserService
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly UserManager<User>? _userManager;

        public UserService(IUserRepository repository, IFileStorageService fileStorageService, UserManager<User>? userManager = null) : base(repository)
        {
            _fileStorageService = fileStorageService;
            _userManager = userManager;
        }

        public async Task<bool> CanUserLoginAsync(string username, string password)
        {
            var user = await _repository.GetByUsernameAsync(username);

            if (user == null)
            {
                return false;
            }

            // If we have UserManager, use it for password validation
            if (_userManager != null)
            {
                var userEntity = await _userManager.FindByNameAsync(username);
                return userEntity != null && await _userManager.CheckPasswordAsync(userEntity, password);
            }

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
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
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
            if (_userManager != null)
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
                    user.UpdatedAt = DateTime.UtcNow;
                    
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
    }
} 