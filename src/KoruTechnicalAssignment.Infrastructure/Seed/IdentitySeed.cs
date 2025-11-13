using KoruTechnicalAssignment.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace KoruTechnicalAssignment.Infrastructure.Seed {
    public static class IdentitySeed {
        public static async Task IdentitySeedAsync(this IServiceProvider sp) {
            var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();

            var roles = new[] { "Admin", "User" };
            foreach (var r in roles)
                if (!await roleManager.RoleExistsAsync(r))
                    await roleManager.CreateAsync(new IdentityRole(r));

            var admins = new[]
            {
                new ApplicationUser { UserName = "admin1@koru.local", Email = "admin1@koru.local", EmailConfirmed = true },
                new ApplicationUser { UserName = "admin2@koru.local", Email = "admin2@koru.local", EmailConfirmed = true }
            };

            foreach (var u in admins) {
                var exists = await userManager.FindByEmailAsync(u.Email!);
                if (exists == null) {
                    await userManager.CreateAsync(u, "Admin123$");
                    await userManager.AddToRoleAsync(u, "Admin");
                }
            }

            var users = new[]
            {
                new ApplicationUser { UserName = "user1@koru.local", Email = "user1@koru.local", EmailConfirmed = true },
                new ApplicationUser { UserName = "user2@koru.local", Email = "user2@koru.local", EmailConfirmed = true },
                new ApplicationUser { UserName = "user3@koru.local", Email = "user3@koru.local", EmailConfirmed = true },
                new ApplicationUser { UserName = "user4@koru.local", Email = "user4@koru.local", EmailConfirmed = true }
            };

            foreach (var u in users) {
                var exists = await userManager.FindByEmailAsync(u.Email!);
                if (exists == null) {
                    await userManager.CreateAsync(u, "User123$");
                    await userManager.AddToRoleAsync(u, "User");
                }
            }
        }
    }
}
