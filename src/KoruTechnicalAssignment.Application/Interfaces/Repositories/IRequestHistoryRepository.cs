using KoruTechnicalAssignment.Domain.Entities.Db;

namespace KoruTechnicalAssignment.Application.Interfaces.Repositories {
    public interface IRequestHistoryRepository {
        Task<IReadOnlyList<RequestStatusHistory>> GetByRequestIdAsync(
                    Guid requestId,
                    CancellationToken cancellationToken = default);

        Task AddAsync(RequestStatusHistory history, CancellationToken cancellationToken = default);
    }
}
