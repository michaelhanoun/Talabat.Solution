using Microsoft.AspNetCore.Identity;
using Talabat.Core.Entities.Identity;

namespace Talabat.Infrastructure._Identity
{
    public static class ApplicationIdentityDataSeed
    {
        public static async Task SeedUsersAsync(UserManager<ApplicationUser>userManager)
        {
            if (!userManager.Users.Any())
            {
                var user = new ApplicationUser()
                {
                    DisplayName = "Michael Hanoun",
                    Email = "michelhanoun210@gmail.com",
                    UserName = "michael.hanoun",
                    PhoneNumber = "01xxxxxxxxx",
                };

                await userManager.CreateAsync(user, "P@ssw0rd");
            }
        }
    }
}
