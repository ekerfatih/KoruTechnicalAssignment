using KoruTechnicalAssignment.Domain.Entities;
using KoruTechnicalAssignment.Domain.Entities.Db;
using KoruTechnicalAssignment.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace KoruTechnicalAssignment.Infrastructure.Persistence {
    public class ApplicationDbContext(DbContextOptions options) : DbContext(options) {
        public DbSet<Branch> Branches => Set<Branch>();
        public DbSet<Request> Requests => Set<Request>();
        public DbSet<RequestStatusHistory> RequestStatusHistories => Set<RequestStatusHistory>();

        protected override void OnModelCreating(ModelBuilder b) {
            base.OnModelCreating(b);

            b.HasDefaultSchema("app");

            b.Entity<Entity>()
                .UseTpcMappingStrategy();

            b.Entity<Entity>()
                .Property(x => x.RowVersion)
                .IsRowVersion();

            b.Entity<ApplicationUser>(e => {
                e.ToTable("AspNetUsers", "auth", t => t.ExcludeFromMigrations());
            });

            // Branch
            b.Entity<Branch>(e => {
                e.ToTable("Branches");

                e.Property(x => x.Name).HasMaxLength(150).IsRequired();
                e.Property(x => x.Location).HasMaxLength(200).IsRequired();

                e.HasMany(x => x.Requests)
                    .WithOne(x => x.Branch)
                    .HasForeignKey(x => x.BranchId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Request
            b.Entity<Request>(e => {
                e.ToTable("Requests");

                e.Property(x => x.Title)
                    .HasMaxLength(200)
                    .IsRequired();

                e.HasIndex(x => new { x.Status, x.RequestDate });

                e.HasOne(x => x.Branch)
                    .WithMany(x => x.Requests)
                    .HasForeignKey(x => x.BranchId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Requester)
                    .WithMany()
                    .HasForeignKey(x => x.RequesterId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasMany(x => x.History)
                    .WithOne(x => x.Request)
                    .HasForeignKey(x => x.RequestId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // RequestStatusHistory
            b.Entity<RequestStatusHistory>(e => {
                e.ToTable("RequestStatusHistories");

                e.HasOne(x => x.Request)
                    .WithMany(x => x.History)
                    .HasForeignKey(x => x.RequestId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.ChangedBy)
                    .WithMany()
                    .HasForeignKey(x => x.ChangedById)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

    }
}
