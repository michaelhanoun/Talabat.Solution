using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Talabat.Core;
using Talabat.Core.Entities.Identity;
using Talabat.Core.Models;
using Talabat.Core.Repositories.Contract;
using Talabat.Core.Services.Contract;
using Talabat.Core.Specifications;

namespace Talabat.Application.AuthService
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuthUnitOfWork _unitOfWork;

        public AuthService(IConfiguration configuration,UserManager<ApplicationUser>userManager,IAuthUnitOfWork unitOfWork)
        {
            _configuration = configuration;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        public async Task<AuthResponse?> AddRefreshTokenToUser(string userId)
        {
            var user = await _userManager.Users.Include(U=>U.RefreshTokens).FirstOrDefaultAsync(U => U.Id == userId);
            if(user is null) return null;
            CleanupOldRefreshTokens(user);
            var newToken = CreateRefreshToken();
            var token = newToken.Token;
            newToken.Token = HashToken(token);
            newToken.ApplicationUserId = userId;
            user.RefreshTokens.Add(newToken);
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return null;
            var jwtToken = await CreateTokenAsync(user);
            return new(){JwtToken=jwtToken , RefreshToken = token,ExpireOfRefreshToken = newToken.Expires};
            
        }
        private void CleanupOldRefreshTokens(ApplicationUser user)
        {
            var expiredTokens = user.RefreshTokens.Where(U => !U.IsActive).ToList();
            foreach (var item in expiredTokens)
            {
                user.RefreshTokens.Remove(item);
            }
            var max = int.Parse(_configuration["Refresh:MaxRefreshTokens"] ?? "5");
            while (user.RefreshTokens.Count >= max)
            {
               var refreshToken = user.RefreshTokens.OrderBy(U => U.Created).FirstOrDefault();
                if(refreshToken != null) 
                user.RefreshTokens.Remove(refreshToken);
            }
        }

        private RefreshToken CreateRefreshToken()
        {
            var randomByte = RandomNumberGenerator.GetBytes(64);
            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomByte),
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(int.Parse(_configuration["Refresh:DurationInDays"] ?? "7"))
            };
        }

        public async Task<string> CreateTokenAsync(ApplicationUser user)
        {
            // Private Claims (User-Defined)
            var authClaims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name,user.DisplayName),
                new Claim(ClaimTypes.Email,user.Email),
                new Claim(ClaimTypes.NameIdentifier,user.Id),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
            };
            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim( ClaimTypes.Role, role));
            }
            var authKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:AuthKey"]??string.Empty));
            var token = new JwtSecurityToken(
                audience: _configuration["JWT:ValidAudience"],
                issuer: _configuration["JWT:ValidIssuer"],
                expires:DateTime.Now.AddDays(double.Parse(_configuration["JWT:DurationInDays"]??"0") ),
                claims:authClaims,
                signingCredentials: new SigningCredentials(authKey,SecurityAlgorithms.HmacSha256Signature)

                );
           return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public string HashToken(string token)
        {
            var keyBytes = Encoding.UTF8.GetBytes(_configuration["JWT:AuthKey"]??"");
            using var hmac = new HMACSHA256(keyBytes);
            var tokenBytes = Encoding.UTF8.GetBytes(token);
            var hashBytes = hmac.ComputeHash(tokenBytes);
            return Convert.ToBase64String(hashBytes);
        }
        public async Task<AuthResponse?> RefreshAsync(string oldTokenValue)
        {
            var repo = _unitOfWork.Repository<RefreshToken>();
            var hashedToken = HashToken(oldTokenValue);
            var oldToken = await repo.GetWithSpecAsync(new RefreshTokenSpecifications(hashedToken));
            if (oldToken is null) return null;
            if (!oldToken.IsActive)
                return null;
            var user = await _userManager.FindByIdAsync(oldToken.ApplicationUserId);
            if (user == null) return null;
            string token = await RotateRefreshTokenAsync(oldToken,repo);
            if(token is null) return null;
            var JwtToken = await CreateTokenAsync(user);
            return new() { JwtToken = JwtToken, RefreshToken = token ,ExpireOfRefreshToken = DateTime.UtcNow.AddDays(int.Parse(_configuration["Refresh:DurationInDays"] ?? "7")) };
        }
        private async Task<string?> RotateRefreshTokenAsync(RefreshToken oldToken,IGenericRepository<RefreshToken> repo)
        {
            var newRefreshToken = CreateRefreshToken();
            oldToken.Revoked = DateTime.UtcNow;
            var token = newRefreshToken.Token;
            newRefreshToken.Token = HashToken(token);
            newRefreshToken.ApplicationUserId = oldToken.ApplicationUserId;
            oldToken.ReplacedByToken = newRefreshToken.Token;
            await repo.Add(newRefreshToken);
            repo.Update(oldToken);
            var count = await _unitOfWork.CompleteAsync();
            if (count <= 0) return null;
            return token;
        }

        public async Task<bool> Revoke(string token, string userId)
        {
            var repo = _unitOfWork.Repository<RefreshToken>();
            var hashedToken = HashToken(token);
            var refreshToken = await repo.GetWithSpecAsync(new RefreshTokenSpecifications(hashedToken));
            if(refreshToken is null) return false;
            if(userId != refreshToken.ApplicationUserId) return false;
            if(!refreshToken.IsActive) return false;
            refreshToken.Revoked =DateTime.UtcNow;
            repo.Update(refreshToken);
            int count  = await _unitOfWork.CompleteAsync();
            if (count < 1) return false;
            return true;
        }

        public async Task<bool> RevokeAllTokensForUser(ApplicationUser user)
        {
            var refreshTokens = user.RefreshTokens.Where(R=>R.IsActive).ToList();
            foreach (var refreshToken in refreshTokens) 
            {
              refreshToken.Revoked = DateTime.UtcNow;
            }
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }
    }
}
