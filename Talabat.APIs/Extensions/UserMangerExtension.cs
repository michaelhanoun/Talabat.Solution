using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Talabat.Core.Entities.Identity;

namespace Talabat.APIs.Extensions
{
    public static class UserMangerExtension
    {
        public static async Task<ApplicationUser?>FindUserWithAddressAsync(this UserManager<ApplicationUser> userManager,ClaimsPrincipal user)
        {
            var email = user.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
            var returnUser = await userManager.Users.Include(U=>U.Address).FirstOrDefaultAsync(U=>U.NormalizedEmail == email.ToUpper());
            return returnUser;
        }
    }
}
