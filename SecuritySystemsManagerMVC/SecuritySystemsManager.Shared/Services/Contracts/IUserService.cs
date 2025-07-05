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
    }
} 