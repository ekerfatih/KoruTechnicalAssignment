using KoruTechnicalAssignment.Application.Interfaces.Services;
using KoruTechnicalAssignment.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace KoruTechnicalAssignment.Infrastructure.Services;

internal sealed class UserProfileService : IUserProfileService {
    private readonly UserManager<ApplicationUser> userManager;

    public UserProfileService(UserManager<ApplicationUser> userManager) => this.userManager = userManager;

    public async Task<string?> GetEmailAsync(string userId, CancellationToken ct = default) {
        var user = await userManager.FindByIdAsync(userId);
        return user?.Email;
    }
}
