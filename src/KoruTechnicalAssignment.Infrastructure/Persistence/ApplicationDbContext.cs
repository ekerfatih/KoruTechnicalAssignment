using Microsoft.EntityFrameworkCore;

namespace KoruTechnicalAssignment.Infrastructure.Persistence {
    public class ApplicationDbContext(DbContextOptions options) : DbContext(options){

        protected override void OnModelCreating(ModelBuilder b) {
            base.OnModelCreating(b);
            b.HasDefaultSchema("app");
        }
    }

}
