using KoruTechnicalAssignment.Domain.Entities.Db;

namespace KoruTechnicalAssignment.Application.Interfaces.Repositories {
    public interface IBranchRepository {
        Task<IReadOnlyList<Branch>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Branch?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
