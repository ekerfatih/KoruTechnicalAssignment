using KoruTechnicalAssignment.Domain.Entities.Db;
using KoruTechnicalAssignment.Domain.Entities.Enums;
using KoruTechnicalAssignment.Domain.Entities.Identity;
using KoruTechnicalAssignment.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KoruTechnicalAssignment.Infrastructure.Seed {
    public static class DataSeed {
        public static async Task SeedAsync(IServiceProvider sp) {
            var db = sp.GetRequiredService<ApplicationDbContext>();
            var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();

            if (!await db.Branches.AnyAsync()) {
                db.Branches.AddRange(
                    new Branch { Name = "Merkez", Location = "Istanbul/Nisantasi" },
                    new Branch { Name = "Ankara", Location = "Ankara/Mamak" },
                    new Branch { Name = "Izmir", Location = "Izmir/Buca" },
                    new Branch { Name = "Kocaeli", Location = "Kocaeli/Gebze" },
                    new Branch { Name = "Hatay", Location = "Hatay/Samandag" }
                );
                await db.SaveChangesAsync();
            }

            if (await db.Requests.AnyAsync())
                return;

            var users = await userManager.GetUsersInRoleAsync("User");
            var branches = await db.Branches.AsNoTracking().OrderBy(b => b.Name).ToListAsync();
            var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

            foreach (var u in users) {
                for (int i = 0; i < branches.Count; i++) {
                    var br = branches[i];
                    var startHour = 9 + (i % 5);
                    var r = new Request {
                        Title = $"{br.Name} Gorusmesi",
                        Description = $"{br.Name} icin planlama",
                        BranchId = br.Id,
                        RequesterId = u.Id,
                        RequestDate = today.AddDays(i + 1),
                        StartTime = new TimeOnly(startHour, 0),
                        EndTime = new TimeOnly(startHour + 1, 0),
                        Status = RequestStatus.Pending
                    };

                    r.History.Add(new RequestStatusHistory {
                        Request = r,
                        Status = RequestStatus.Pending,
                        ChangedById = u.Id
                    });

                    db.Requests.Add(r);
                }
            }

            await db.SaveChangesAsync();
        }
    }
}
