using KoruTechnicalAssignment.Application.Interfaces.Repositories;
using KoruTechnicalAssignment.Domain.Entities.Db;
using KoruTechnicalAssignment.Domain.Entities.Enums;
using KoruTechnicalAssignment.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KoruTechnicalAssignment.Infrastructure.Repositories;

internal sealed class RequestRepository : IRequestRepository {
    private readonly ApplicationDbContext db;

    public RequestRepository(ApplicationDbContext db) => this.db = db;

    public Task<Request?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Requests
          .Include(r => r.Branch)
          .Include(r => r.Requester)
          .Include(r => r.History)
              .ThenInclude(h => h.ChangedBy)
          .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<IReadOnlyList<Request>> GetUserRequestsAsync(
        string userId,
        RequestStatus? status,
        DateOnly? startDate,
        DateOnly? endDate,
        string? search,
        int page,
        int pageSize,
        RequestSortField sortBy,
        SortDirection sortDirection,
        CancellationToken ct = default) {
        var query = BaseQuery()
            .Where(r => r.RequesterId == userId);

        query = ApplyFilters(query, status, startDate, endDate, search);

        return await ApplyPaging(query, sortBy, sortDirection, page, pageSize)
            .ToListAsync(ct);
    }

    public Task<int> CountUserRequestsAsync(
        string userId,
        RequestStatus? status,
        DateOnly? startDate,
        DateOnly? endDate,
        string? search,
        CancellationToken ct = default) {
        var query = db.Requests.Where(r => r.RequesterId == userId);
        query = ApplyFilters(query, status, startDate, endDate, search);
        return query.CountAsync(ct);
    }

    public async Task<IReadOnlyList<Request>> GetPendingRequestsAsync(
        DateOnly? startDate,
        DateOnly? endDate,
        string? search,
        int page,
        int pageSize,
        RequestSortField sortBy,
        SortDirection sortDirection,
        CancellationToken ct = default) {
        var query = BaseQuery()
            .Where(r => r.Status == RequestStatus.Pending);

        query = ApplyDateAndSearch(query, startDate, endDate, search);

        return await ApplyPaging(query, sortBy, sortDirection, page, pageSize).ToListAsync(ct);
    }

    public Task<int> CountPendingRequestsAsync(
        DateOnly? startDate,
        DateOnly? endDate,
        string? search,
        CancellationToken ct = default) {
        var query = db.Requests.Where(r => r.Status == RequestStatus.Pending);
        query = ApplyDateAndSearch(query, startDate, endDate, search);
        return query.CountAsync(ct);
    }

    public async Task AddAsync(Request request, CancellationToken ct = default) {
        await db.Requests.AddAsync(request, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Request request, CancellationToken ct = default) {
        db.Requests.Update(request);
        await db.SaveChangesAsync(ct);
    }

    private IQueryable<Request> BaseQuery() =>
        db.Requests
          .AsNoTracking()
          .Include(r => r.Branch)
          .Include(r => r.Requester);

    private static IQueryable<Request> ApplyFilters(
        IQueryable<Request> query,
        RequestStatus? status,
        DateOnly? startDate,
        DateOnly? endDate,
        string? search) {
        if (status.HasValue)
            query = query.Where(r => r.Status == status);

        return ApplyDateAndSearch(query, startDate, endDate, search);
    }

    private static IQueryable<Request> ApplyDateAndSearch(
        IQueryable<Request> query,
        DateOnly? startDate,
        DateOnly? endDate,
        string? search) {
        if (startDate.HasValue)
            query = query.Where(r => r.RequestDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(r => r.RequestDate <= endDate.Value);

        if (!string.IsNullOrWhiteSpace(search)) {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(r =>
                r.Title.ToLower().Contains(term) ||
                (r.Description ?? string.Empty).ToLower().Contains(term));
        }

        return query;
    }

    private static IQueryable<Request> ApplyPaging(
        IQueryable<Request> query,
        RequestSortField sortBy,
        SortDirection sortDirection,
        int page,
        int pageSize) =>
        ApplySorting(query, sortBy, sortDirection)
            .Skip(Math.Max(page - 1, 0) * pageSize)
            .Take(pageSize);

    private static IOrderedQueryable<Request> ApplySorting(
        IQueryable<Request> query,
        RequestSortField sortBy,
        SortDirection sortDirection) {
        return (sortBy, sortDirection) switch {
            (RequestSortField.Status, SortDirection.Asc) => query.OrderBy(r => r.Status).ThenBy(r => r.RequestDate).ThenBy(r => r.StartTime),
            (RequestSortField.Status, SortDirection.Desc) => query.OrderByDescending(r => r.Status).ThenByDescending(r => r.RequestDate).ThenByDescending(r => r.StartTime),
            (RequestSortField.Date, SortDirection.Asc) => query.OrderBy(r => r.RequestDate).ThenBy(r => r.StartTime),
            _ => query.OrderByDescending(r => r.RequestDate).ThenByDescending(r => r.StartTime)
        };
    }
}
