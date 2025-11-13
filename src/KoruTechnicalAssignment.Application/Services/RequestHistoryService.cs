using KoruTechnicalAssignment.Application.DTO;
using KoruTechnicalAssignment.Application.Interfaces.Repositories;
using KoruTechnicalAssignment.Application.Interfaces.Services;
using KoruTechnicalAssignment.Domain.Entities.Db;

namespace KoruTechnicalAssignment.Application.Services;

public sealed class RequestStatusHistoryService : IRequestStatusHistoryService {
    private readonly IRequestHistoryRepository histories;

    public RequestStatusHistoryService(IRequestHistoryRepository histories) => this.histories = histories;

    public async Task<IReadOnlyList<RequestStatusHistoryDto>> GetByRequestIdAsync(
        Guid requestId,
        CancellationToken ct = default) {
        var items = await histories.GetByRequestIdAsync(requestId, ct);
        return items
            .Select(MapToDto)
            .ToList();
    }

    public async Task AddEntryAsync(
        RequestStatusHistoryCreateDto dto,
        CancellationToken ct = default) {
        var entity = new RequestStatusHistory {
            RequestId = dto.RequestId,
            Status = dto.Status,
            ChangedById = dto.ChangedById,
            Reason = dto.Reason,
            ChangedAt = DateTime.UtcNow
        };

        await histories.AddAsync(entity, ct);
    }

    private static RequestStatusHistoryDto MapToDto(RequestStatusHistory history) =>
        new() {
            Id = history.Id,
            Status = history.Status,
            ChangedBy = history.ChangedBy?.Email ?? history.ChangedById,
            Reason = history.Reason,
            ChangedAt = history.ChangedAt
        };
}
