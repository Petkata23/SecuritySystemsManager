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
        private readonly IHostEnvironment _hostEnvironment;

        public UserService(IUserRepository repository, IHostEnvironment hostEnvironment) : base(repository)
        {
            _hostEnvironment = hostEnvironment;
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

            // Create uploads directory if it doesn't exist
            string uploadsFolder = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot", "uploads", "users");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generate unique filename
            string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(imageFile.FileName)}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Save file
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            // Return relative path for storage in database
            return Path.Combine("uploads", "users", uniqueFileName).Replace("\\", "/");
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