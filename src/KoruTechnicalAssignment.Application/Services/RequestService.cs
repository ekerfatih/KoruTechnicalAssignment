using KoruTechnicalAssignment.Application.DTO;
using KoruTechnicalAssignment.Application.Interfaces.Repositories;
using KoruTechnicalAssignment.Application.Interfaces.Services;
using KoruTechnicalAssignment.Domain.Entities.Db;
using KoruTechnicalAssignment.Domain.Entities.Enums;

namespace KoruTechnicalAssignment.Application.Services;

public sealed class RequestService : IRequestService {
    private readonly IRequestRepository requests;
    private readonly IBranchRepository branches;
    private readonly IRequestStatusHistoryService historyService;
    private readonly IRequestHistoryRepository historyRepository;

    public RequestService(
        IRequestRepository requests,
        IBranchRepository branches,
        IRequestStatusHistoryService historyService,
        IRequestHistoryRepository historyRepository) {
        this.requests = requests;
        this.branches = branches;
        this.historyService = historyService;
        this.historyRepository = historyRepository;
    }

    public async Task<Guid> CreateDraftAsync(
        RequestCreateDto dto,
        string userId,
        CancellationToken ct = default) {
        await EnsureBranchExists(dto.BranchId, ct);

        var entity = new Request {
            BranchId = dto.BranchId,
            RequesterId = userId,
            Title = dto.Title,
            Description = dto.Description,
            RequestDate = dto.RequestDate,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Status = RequestStatus.Draft
        };

        await requests.AddAsync(entity, ct);
        await historyService.AddEntryAsync(
            new RequestStatusHistoryCreateDto {
                RequestId = entity.Id,
                ChangedById = userId,
                Status = RequestStatus.Draft,
                Reason = "Taslak oluşturuldu"
            }, ct);

        return entity.Id;
    }

    public async Task UpdateDraftAsync(
        Guid requestId,
        RequestUpdateDto dto,
        string userId,
        CancellationToken ct = default) {
        var entity = await RequireOwnedDraftAsync(requestId, userId, ct);

        await EnsureBranchExists(dto.BranchId, ct);

        entity.BranchId = dto.BranchId;
        entity.Title = dto.Title;
        entity.Description = dto.Description;
        entity.RequestDate = dto.RequestDate;
        entity.StartTime = dto.StartTime;
        entity.EndTime = dto.EndTime;

        await requests.UpdateAsync(entity, ct);
    }

    public async Task SubmitAsync(Guid requestId, string userId, CancellationToken ct = default) {
        var entity = await RequireOwnedDraftAsync(requestId, userId, ct);
        entity.Status = RequestStatus.Pending;

        await requests.UpdateAsync(entity, ct);
        await historyService.AddEntryAsync(new RequestStatusHistoryCreateDto {
            RequestId = entity.Id,
            ChangedById = userId,
            Status = RequestStatus.Pending,
            Reason = "Talep gönderildi"
        }, ct);
    }

    public async Task<(IReadOnlyList<RequestListItemDto> Items, int TotalCount)> GetUserRequestsAsync(
        string userId,
        RequestStatus? status,
        DateOnly? startDate,
        DateOnly? endDate,
        string? search,
        int page,
        int pageSize,
        RequestSortField sortBy = RequestSortField.Date,
        SortDirection sortDirection = SortDirection.Desc,
        CancellationToken ct = default) {
        var total = await requests.CountUserRequestsAsync(userId, status, startDate, endDate, search, ct);
        if (total == 0)
            return (Array.Empty<RequestListItemDto>(), 0);

        var items = await requests.GetUserRequestsAsync(userId, status, startDate, endDate, search, page, pageSize, sortBy, sortDirection, ct);
        return (items.Select(MapToListDto).ToList(), total);
    }

    public async Task<(IReadOnlyList<RequestListItemDto> Items, int TotalCount)> GetPendingRequestsAsync(
        DateOnly? startDate,
        DateOnly? endDate,
        string? search,
        int page,
        int pageSize,
        RequestSortField sortBy = RequestSortField.Date,
        SortDirection sortDirection = SortDirection.Desc,
        CancellationToken ct = default) {
        var total = await requests.CountPendingRequestsAsync(startDate, endDate, search, ct);
        if (total == 0)
            return (Array.Empty<RequestListItemDto>(), 0);

        var items = await requests.GetPendingRequestsAsync(startDate, endDate, search, page, pageSize, sortBy, sortDirection, ct);
        return (items.Select(MapToListDto).ToList(), total);
    }

    public async Task<RequestDetailDto?> GetByIdForUserAsync(
        Guid requestId,
        string userId,
        CancellationToken ct = default) {
        var entity = await requests.GetByIdAsync(requestId, ct);
        if (entity is null || entity.RequesterId != userId)
            return null;

        return await MapToDetailDtoAsync(entity, ct);
    }

    public async Task<RequestDetailDto?> GetByIdForAdminAsync(
        Guid requestId,
        CancellationToken ct = default) {
        var entity = await requests.GetByIdAsync(requestId, ct);
        return entity is null ? null : await MapToDetailDtoAsync(entity, ct);
    }

    public async Task ApproveAsync(
        Guid requestId,
        string adminUserId,
        string? reason,
        CancellationToken ct = default) {
        var entity = await RequirePendingRequestAsync(requestId, ct);
        entity.Status = RequestStatus.Approved;

        await requests.UpdateAsync(entity, ct);
        await historyService.AddEntryAsync(new RequestStatusHistoryCreateDto {
            RequestId = entity.Id,
            ChangedById = adminUserId,
            Status = RequestStatus.Approved,
            Reason = reason
        }, ct);
    }

    public async Task RejectAsync(
        Guid requestId,
        string adminUserId,
        string reason,
        CancellationToken ct = default) {
        var entity = await RequirePendingRequestAsync(requestId, ct);
        entity.Status = RequestStatus.Rejected;

        await requests.UpdateAsync(entity, ct);
        await historyService.AddEntryAsync(new RequestStatusHistoryCreateDto {
            RequestId = entity.Id,
            ChangedById = adminUserId,
            Status = RequestStatus.Rejected,
            Reason = reason
        }, ct);
    }

    private async Task EnsureBranchExists(Guid branchId, CancellationToken ct) {
        if (await branches.GetByIdAsync(branchId, ct) is null)
            throw new InvalidOperationException("Şube bulunamadı.");
    }

    private async Task<Request> RequireOwnedDraftAsync(Guid id, string userId, CancellationToken ct) {
        var entity = await requests.GetByIdAsync(id, ct);
        if (entity is null || entity.RequesterId != userId)
            throw new InvalidOperationException("Talep bulunamadı.");

        if (entity.Status != RequestStatus.Draft)
            throw new InvalidOperationException("Sadece taslak talepler güncellenebilir.");

        return entity;
    }

    private async Task<Request> RequirePendingRequestAsync(Guid id, CancellationToken ct) {
        var entity = await requests.GetByIdAsync(id, ct)
            ?? throw new InvalidOperationException("Talep bulunamadı.");

        if (entity.Status != RequestStatus.Pending)
            throw new InvalidOperationException("Talep bekleme durumunda değil.");

        return entity;
    }

    private async Task<RequestDetailDto> MapToDetailDtoAsync(Request request, CancellationToken ct) {
        var history = await historyRepository.GetByRequestIdAsync(request.Id, ct);

        return new RequestDetailDto {
            Id = request.Id,
            BranchId = request.BranchId,
            BranchName = request.Branch?.Name ?? string.Empty,
            Title = request.Title,
            Description = request.Description,
            RequesterName = request.Requester?.Email ?? request.RequesterId,
            RequestDate = request.RequestDate,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Status = request.Status,
            History = history.Select(h => new RequestStatusHistoryDto {
                Id = h.Id,
                Status = h.Status,
                ChangedBy = h.ChangedBy?.Email ?? h.ChangedById,
                Reason = h.Reason,
                ChangedAt = h.ChangedAt
            }).ToList()
        };
    }

    private static RequestListItemDto MapToListDto(Request request) =>
        new() {
            Id = request.Id,
            Title = request.Title,
            RequesterName = request.Requester?.Email ?? request.RequesterId,
            RequestDate = request.RequestDate,
            StartTime = request.StartTime,
            Status = request.Status
        };
}
