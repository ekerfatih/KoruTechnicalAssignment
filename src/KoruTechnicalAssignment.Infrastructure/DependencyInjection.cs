using KoruTechnicalAssignment.Application.Interfaces.Repositories;
using KoruTechnicalAssignment.Application.Interfaces.Services;
using KoruTechnicalAssignment.Application.Services;
using KoruTechnicalAssignment.Domain.Entities.Identity;
using KoruTechnicalAssignment.Infrastructure.Persistence;
using KoruTechnicalAssignment.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KoruTechnicalAssignment.Infrastructure;

public static class DependencyInjection {
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration cfg) {
        var cs = cfg.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationIdentityDbContext>(o => o.UseSqlServer(cs));
        services.AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(cs));

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

        services.AddHttpContextAccessor();

        return services;
    }
}
