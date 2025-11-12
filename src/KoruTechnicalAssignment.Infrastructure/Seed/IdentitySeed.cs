using KoruTechnicalAssignment.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace KoruTechnicalAssignment.Infrastructure.Identity;

public static class IdentitySeed {
    public static async Task IdentitySeedAsync(this IServiceProvider serviceProvider) {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roles = { "Admin", "User" };
        foreach (var role in roles)
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

        var adminEmail = "admin@example.com";
        var adminPassword = "Admin123!";

        if (await userManager.FindByEmailAsync(adminEmail) == null) {
            var admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            await userManager.CreateAsync(admin, adminPassword);
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}
