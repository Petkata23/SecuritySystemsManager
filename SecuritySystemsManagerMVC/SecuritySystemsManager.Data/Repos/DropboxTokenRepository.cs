using SecuritySystemsManager.Data.Entities;
using SecuritySystemsManager.Shared.Attributes;
using SecuritySystemsManager.Shared.Repos.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SecuritySystemsManager.Data.Repos
{
    [AutoBind]
    public class DropboxTokenRepository : IDropboxTokenRepository
    {
        protected readonly SecuritySystemsManagerDbContext _dbContext;

        public DropboxTokenRepository(SecuritySystemsManagerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<(string AccessToken, string RefreshToken, DateTime ExpiryTime)> GetLatestTokenAsync()
        {
            var token = await _dbContext.DropboxTokens
                .OrderByDescending(t => t.UpdatedAt)
                .FirstOrDefaultAsync();
                
            if (token != null)
            {
                return (token.AccessToken, token.RefreshToken, token.ExpiryTime);
            }
            
            return (null, null, DateTime.MinValue);
        }

        public async Task SaveTokenAsync(string accessToken, string refreshToken, DateTime expiryTime)
        {
            var existingToken = await _dbContext.DropboxTokens
                .OrderByDescending(t => t.UpdatedAt)
                .FirstOrDefaultAsync();

            if (existingToken != null)
            {
                existingToken.AccessToken = accessToken;
                existingToken.RefreshToken = refreshToken;
                existingToken.ExpiryTime = expiryTime;
                existingToken.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var newToken = new DropboxToken
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiryTime = expiryTime,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _dbContext.DropboxTokens.Add(newToken);
            }
            
            await _dbContext.SaveChangesAsync();
        }
    }
} 