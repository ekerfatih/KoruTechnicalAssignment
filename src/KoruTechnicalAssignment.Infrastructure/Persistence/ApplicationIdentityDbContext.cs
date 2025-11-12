using KoruTechnicalAssignment.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KoruTechnicalAssignment.Infrastructure.Persistence {
    public class ApplicationIdentityDbContext(DbContextOptions<ApplicationIdentityDbContext> options) : IdentityDbContext<ApplicationUser>(options) {

        protected override void OnModelCreating(ModelBuilder b) {
            base.OnModelCreating(b);
            b.HasDefaultSchema("auth");
        }
    }
}