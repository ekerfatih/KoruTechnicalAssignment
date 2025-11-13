using KoruTechnicalAssignment.Application.Interfaces.Repositories;
using KoruTechnicalAssignment.Domain.Entities.Db;
using KoruTechnicalAssignment.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KoruTechnicalAssignment.Infrastructure.Repositories;

public class BranchRepository : IBranchRepository {
    private readonly ApplicationDbContext db;

    public BranchRepository(ApplicationDbContext db) => this.db = db;

    public async Task<IReadOnlyList<Branch>> GetAllAsync(CancellationToken ct = default) =>
        await db.Branches.AsNoTracking().OrderBy(b => b.Name).ToListAsync(ct);

    public async Task<Branch?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
       await db.Branches.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id, ct);
}
