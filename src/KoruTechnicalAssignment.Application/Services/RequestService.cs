using KoruTechnicalAssignment.Application.DTO;
using KoruTechnicalAssignment.Application.Interfaces;
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
    private readonly IUnitOfWork unitOfWork;

    public RequestService(
        IRequestRepository requests,
        IBranchRepository branches,
        IRequestStatusHistoryService historyService,
        IRequestHistoryRepository historyRepository,
        IUnitOfWork unitOfWork) {
        this.requests = requests;
        this.branches = branches;
        this.historyService = historyService;
        this.historyRepository = historyRepository;
        this.unitOfWork = unitOfWork;
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
                Reason = "Draft created"
            }, ct);
        await unitOfWork.SaveChangesAsync(ct);

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
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task SubmitAsync(Guid requestId, string userId, CancellationToken ct = default) {
        var entity = await RequireOwnedDraftAsync(requestId, userId, ct);
        entity.Status = RequestStatus.Pending;

        await requests.UpdateAsync(entity, ct);
        await historyService.AddEntryAsync(new RequestStatusHistoryCreateDto {
            RequestId = entity.Id,
            ChangedById = userId,
            Status = RequestStatus.Pending,
            Reason = "Request submitted"
        }, ct);
        await unitOfWork.SaveChangesAsync(ct);
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

    public async Task<(IReadOnlyList<RequestListItemDto> Items, int TotalCount)> GetAdminRequestsAsync(
        RequestStatus? status,
        DateOnly? startDate,
        DateOnly? endDate,
        string? search,
        int page,
        int pageSize,
        RequestSortField sortBy = RequestSortField.Date,
        SortDirection sortDirection = SortDirection.Desc,
        CancellationToken ct = default) {
        var total = await requests.CountAdminRequestsAsync(status, startDate, endDate, search, ct);
        if (total == 0)
            return (Array.Empty<RequestListItemDto>(), 0);

        var items = await requests.GetAdminRequestsAsync(status, startDate, endDate, search, page, pageSize, sortBy, sortDirection, ct);
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
        await unitOfWork.SaveChangesAsync(ct);
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
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task ReopenAsync(
        Guid requestId,
        string userId,
        CancellationToken ct = default) {
        var entity = await requests.GetByIdAsync(requestId, ct)
            ?? throw new InvalidOperationException("Request not found.");

        if (entity.RequesterId != userId)
            throw new InvalidOperationException("Request not found.");

        if (entity.Status != RequestStatus.Rejected)
            throw new InvalidOperationException("Only rejected requests can be reopened.");

        entity.Status = RequestStatus.Draft;

        await requests.UpdateAsync(entity, ct);
        await historyService.AddEntryAsync(new RequestStatusHistoryCreateDto {
            RequestId = entity.Id,
            ChangedById = userId,
            Status = RequestStatus.Draft,
            Reason = "Request reopened"
        }, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    private async Task EnsureBranchExists(Guid branchId, CancellationToken ct) {
        if (await branches.GetByIdAsync(branchId, ct) is null)
            throw new InvalidOperationException("Branch not found.");
    }

    private async Task<Request> RequireOwnedDraftAsync(Guid id, string userId, CancellationToken ct) {
        var entity = await requests.GetByIdAsync(id, ct);
        if (entity is null || entity.RequesterId != userId)
            throw new InvalidOperationException("Request not found.");

        if (entity.Status != RequestStatus.Draft)
            throw new InvalidOperationException("Only draft requests can be updated.");

        return entity;
    }

    private async Task<Request> RequirePendingRequestAsync(Guid id, CancellationToken ct) {
        var entity = await requests.GetByIdAsync(id, ct)
            ?? throw new InvalidOperationException("Request not found.");

        if (entity.Status != RequestStatus.Pending)
            throw new InvalidOperationException("Request is not pending.");

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
