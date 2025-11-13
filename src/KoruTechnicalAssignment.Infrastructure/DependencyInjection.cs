using KoruTechnicalAssignment.Application.Interfaces.Repositories;
using KoruTechnicalAssignment.Application.Interfaces.Services;
using KoruTechnicalAssignment.Application.Services;
using KoruTechnicalAssignment.Domain.Entities.Identity;
using KoruTechnicalAssignment.Infrastructure.Persistence;
using KoruTechnicalAssignment.Infrastructure.Repositories;
using KoruTechnicalAssignment.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KoruTechnicalAssignment.Infrastructure;

public static class DependencyInjection {
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration cfg) {
        var defaultCs = cfg.GetConnectionString("DefaultConnection")
                      ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is missing.");
        var identityCs = cfg.GetConnectionString("IdentityConnection")
                       ?? throw new InvalidOperationException("ConnectionStrings:IdentityConnection is missing.");

        services.AddDbContext<ApplicationIdentityDbContext>(o => o.UseNpgsql(identityCs));
        services.AddDbContext<ApplicationDbContext>(o => o.UseNpgsql(defaultCs));

        services.AddIdentityCore<ApplicationUser>(o => o.SignIn.RequireConfirmedAccount = true)
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationIdentityDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        // repositories
        services.AddScoped<IBranchRepository, BranchRepository>();
        services.AddScoped<IRequestRepository, RequestRepository>();
        services.AddScoped<IRequestHistoryRepository, RequestHistoryRepository>();

        // services
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<IRequestService, RequestService>();
        services.AddScoped<IRequestStatusHistoryService, RequestStatusHistoryService>();
        services.AddScoped<IUserProfileService, UserProfileService>();

        services.AddHttpContextAccessor();

        return services;
    }
}
