using System;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Shared.Repos.Contracts
{
    public interface IDropboxTokenRepository
    {
        Task<(string AccessToken, string RefreshToken, DateTime ExpiryTime)> GetLatestTokenAsync();
        Task SaveTokenAsync(string accessToken, string refreshToken, DateTime expiryTime);
    }
} 