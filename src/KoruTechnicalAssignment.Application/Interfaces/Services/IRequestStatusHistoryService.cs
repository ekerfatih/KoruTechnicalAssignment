
using KoruTechnicalAssignment.Application.DTO;

namespace KoruTechnicalAssignment.Application.Interfaces.Services {
    public interface IRequestStatusHistoryService {
        Task<IReadOnlyList<RequestStatusHistoryDto>> GetByRequestIdAsync(
            Guid requestId,
            CancellationToken cancellationToken = default);

        Task AddEntryAsync(
            RequestStatusHistoryCreateDto dto,
            CancellationToken cancellationToken = default);
    }
}
