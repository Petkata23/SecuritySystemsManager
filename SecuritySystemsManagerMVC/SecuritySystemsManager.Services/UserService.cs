using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
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

        public UserService(IUserRepository repository, IFileStorageService fileStorageService) : base(repository)
        {
            _fileStorageService = fileStorageService;
        }

        public async Task<bool> CanUserLoginAsync(string username, string password)
        {
            var user = await _repository.GetByUsernameAsync(username);

            if (user == null)
            {
                return false;
            }

            bool passwordMatches = PasswordHasher.VerifyPassword(password, user.Password);

            return passwordMatches;
        }

        public Task<UserDto> GetByUsernameAsync(string username)
        {
            return _repository.GetByUsernameAsync(username);
        }
        
        public async Task<UserDto> CreateUserWithPasswordAsync(UserDto userDto, string password)
        {
            // Hash the password before saving
            userDto.Password = PasswordHasher.HashPassword(password);
            
            // Save the user
            await SaveAsync(userDto);
            
            return userDto;
        }
        
        public async Task<UserDto> UpdateUserWithPasswordAsync(UserDto userDto, string password = null)
        {
            // If password is provided, hash and update it
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
            
            // Save the user
            await SaveAsync(userDto);
            
            return userDto;
        }
        
        public async Task<string> UploadUserProfileImageAsync(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return null;

            // Use the file storage service to upload the file
            return await _fileStorageService.UploadFileAsync(imageFile, "uploads/users");
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
            
            // Hash password and save user
            return await CreateUserWithPasswordAsync(userDto, password);
        }
    }
} 