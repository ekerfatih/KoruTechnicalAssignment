using KoruTechnicalAssignment.Application.Interfaces;
using KoruTechnicalAssignment.Infrastructure.Persistence;

namespace KoruTechnicalAssignment.Infrastructure.Common;

internal sealed class UnitOfWork(ApplicationDbContext dbContext) : IUnitOfWork {
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
