using KoruTechnicalAssignment.Domain.Entities.Db;
using KoruTechnicalAssignment.Domain.Entities.Enums;

namespace KoruTechnicalAssignment.Application.Interfaces.Repositories {
    public interface IRequestRepository {
        Task<Request?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Request>> GetUserRequestsAsync(
            string userId,
            RequestStatus? status,
            DateOnly? startDate,
            DateOnly? endDate,
            string? search,
            int page,
            int pageSize,
            RequestSortField sortBy,
            SortDirection sortDirection,
            CancellationToken cancellationToken = default);

        Task<int> CountUserRequestsAsync(
            string userId,
            RequestStatus? status,
            DateOnly? startDate,
            DateOnly? endDate,
            string? search,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Request>> GetPendingRequestsAsync(
            DateOnly? startDate,
            DateOnly? endDate,
            string? search,
            int page,
            int pageSize,
            RequestSortField sortBy,
            SortDirection sortDirection,
            CancellationToken cancellationToken = default);

        Task<int> CountPendingRequestsAsync(
            DateOnly? startDate,
            DateOnly? endDate,
            string? search,
            CancellationToken cancellationToken = default);

        Task AddAsync(Request request, CancellationToken cancellationToken = default);
        Task UpdateAsync(Request request, CancellationToken cancellationToken = default);
    }
}
