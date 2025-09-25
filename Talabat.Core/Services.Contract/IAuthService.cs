using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Talabat.Core.Entities.Identity;
using Talabat.Core.Models;

namespace Talabat.Core.Services.Contract
{
    public interface IAuthService
    {
        Task<string> CreateTokenAsync(ApplicationUser user);
        Task<AuthResponse> RefreshAsync(string oldTokenValue);
        Task<AuthResponse?> AddRefreshTokenToUser(string userId);
        Task<bool> Revoke(string token, string userId);
        Task<bool> RevokeAllTokensForUser(ApplicationUser user);

    }
}
