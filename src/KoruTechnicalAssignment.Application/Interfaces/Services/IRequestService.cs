using KoruTechnicalAssignment.Application.DTO;
using KoruTechnicalAssignment.Domain.Entities.Enums;

namespace KoruTechnicalAssignment.Application.Interfaces.Services {
    public interface IRequestService {
        Task<Guid> CreateDraftAsync(
            RequestCreateDto dto,
            string userId,
            CancellationToken cancellationToken = default);

        Task UpdateDraftAsync(
            Guid requestId,
            RequestUpdateDto dto,
            string userId,
            CancellationToken cancellationToken = default);

        Task SubmitAsync(
            Guid requestId,
            string userId,
            CancellationToken cancellationToken = default);

        Task<(IReadOnlyList<RequestListItemDto> Items, int TotalCount)> GetUserRequestsAsync(
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

        Task<(IReadOnlyList<RequestListItemDto> Items, int TotalCount)> GetAdminRequestsAsync(
            RequestStatus? status,
            DateOnly? startDate,
            DateOnly? endDate,
            string? search,
            int page,
            int pageSize,
            RequestSortField sortBy,
            SortDirection sortDirection,
            CancellationToken cancellationToken = default);

        Task<RequestDetailDto?> GetByIdForUserAsync(
            Guid requestId,
            string userId,
            CancellationToken cancellationToken = default);

        Task<RequestDetailDto?> GetByIdForAdminAsync(
            Guid requestId,
            CancellationToken cancellationToken = default);

        Task ApproveAsync(
            Guid requestId,
            string adminUserId,
            string? reason,
            CancellationToken cancellationToken = default);

        Task RejectAsync(
            Guid requestId,
            string adminUserId,
            string reason,
            CancellationToken cancellationToken = default);

        Task ReopenAsync(
            Guid requestId,
            string userId,
            CancellationToken cancellationToken = default);
    }
}
