using KoruTechnicalAssignment.Application.Interfaces.Repositories;
using KoruTechnicalAssignment.Domain.Entities.Db;
using KoruTechnicalAssignment.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KoruTechnicalAssignment.Infrastructure.Repositories;

internal sealed class RequestHistoryRepository : IRequestHistoryRepository {
    private readonly ApplicationDbContext db;

    public RequestHistoryRepository(ApplicationDbContext db) => this.db = db;

    public async Task<IReadOnlyList<RequestStatusHistory>> GetByRequestIdAsync(
        Guid requestId,
        CancellationToken ct = default) {
        return await db.RequestStatusHistories
            .AsNoTracking()
            .Include(h => h.ChangedBy)
            .Where(h => h.RequestId == requestId)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync(ct);
    }

    public Task AddAsync(RequestStatusHistory history, CancellationToken ct = default) =>
        db.RequestStatusHistories.AddAsync(history, ct).AsTask();
}
