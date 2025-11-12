using KoruTechnicalAssignment.Domain.Entities;
using KoruTechnicalAssignment.Domain.Entities.Db;
using KoruTechnicalAssignment.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace KoruTechnicalAssignment.Infrastructure.Persistence {
    public class ApplicationDbContext(DbContextOptions options) : DbContext(options) {
        public DbSet<Branch> Branches => Set<Branch>();
        public DbSet<Request> Requests => Set<Request>();
        public DbSet<AppointmentStatusHistory> AppointmentStatusHistories => Set<AppointmentStatusHistory>();

        protected override void OnModelCreating(ModelBuilder b) {
            base.OnModelCreating(b);
            b.HasDefaultSchema("app");
            b.Entity<Entity>().UseTpcMappingStrategy();
            b.Entity<Entity>().Property(x => x.RowVersion).IsRowVersion();

            b.Entity<ApplicationUser>(e => {
                e.ToTable("AspNetUsers", "auth", t => t.ExcludeFromMigrations());
            });

            b.Entity<Branch>().Property(x => x.Name).HasMaxLength(150).IsRequired();
            b.Entity<Branch>().Property(x => x.Location).HasMaxLength(200).IsRequired();

            b.Entity<Request>().Property(x => x.Title).HasMaxLength(200).IsRequired();
            b.Entity<Request>().HasIndex(x => new { x.Status, x.RequestDate });
            b.Entity<Request>().HasOne(x => x.Branch).WithMany(x => x.Requests).HasForeignKey(x => x.BranchId);
            b.Entity<Request>().HasOne(x => x.Requester).WithMany().HasForeignKey(x => x.RequesterId).OnDelete(DeleteBehavior.Restrict);

            b.Entity<AppointmentStatusHistory>().HasOne(x => x.Request).WithMany(x => x.History).HasForeignKey(x => x.RequestId);
            b.Entity<AppointmentStatusHistory>().HasOne(x => x.ChangedBy).WithMany().HasForeignKey(x => x.ChangedById);
            b.Entity<AppointmentStatusHistory>().HasOne(x => x.ChangedBy).WithMany().HasForeignKey(x => x.ChangedById).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
