using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using KoruTechnicalAssignment.Web.Client.Models;

namespace KoruTechnicalAssignment.Web.Client.Services;

public sealed class RequestApiClient
{
    private readonly HttpClient httpClient;

    public RequestApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<IReadOnlyList<BranchModel>> GetBranchesAsync(CancellationToken cancellationToken = default)
    {
        var branches = await httpClient.GetFromJsonAsync<List<BranchModel>>("api/branches", cancellationToken);
        return branches ?? [];
    }

    public Task<RequestListResponse> GetMyRequestsAsync(RequestFiltersModel filters, CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(filters);
        return GetListAsync($"api/requests{query}", cancellationToken);
    }

    public Task<RequestListResponse> GetAdminPendingRequestsAsync(RequestFiltersModel filters, CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(filters);
        return GetListAsync($"api/requests/admin{query}", cancellationToken);
    }

    public Task<RequestDetailModel?> GetRequestForUserAsync(Guid id, CancellationToken cancellationToken = default) =>
        httpClient.GetFromJsonAsync<RequestDetailModel>($"api/requests/{id}", cancellationToken);

    public Task<RequestDetailModel?> GetRequestForAdminAsync(Guid id, CancellationToken cancellationToken = default) =>
        httpClient.GetFromJsonAsync<RequestDetailModel>($"api/requests/admin/{id}", cancellationToken);

    public async Task<Guid> CreateDraftAsync(RequestFormModel model, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/requests", MapToDto(model), cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        var envelope = await response.Content.ReadFromJsonAsync<CreateResponse>(cancellationToken: cancellationToken);
        return envelope?.Id ?? Guid.Empty;
    }

    public async Task UpdateDraftAsync(Guid id, RequestFormModel model, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsJsonAsync($"api/requests/{id}", MapToDto(model), cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task SubmitAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsync($"api/requests/{id}/submit", null, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task ApproveAsync(Guid id, string? reason, CancellationToken cancellationToken = default)
    {
        var content = JsonContent.Create(new ApprovePayload(reason));
        var response = await httpClient.PostAsync($"api/requests/admin/{id}/approve", content, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task RejectAsync(Guid id, string reason, CancellationToken cancellationToken = default)
    {
        var content = JsonContent.Create(new RejectPayload(reason));
        var response = await httpClient.PostAsync($"api/requests/admin/{id}/reject", content, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    private static RequestCreateDto MapToDto(RequestFormModel model) =>
        new()
        {
            BranchId = model.BranchId ?? Guid.Empty,
            Title = model.Title,
            Description = model.Description,
            RequestDate = model.RequestDate!.Value,
            StartTime = model.StartTime!.Value,
            EndTime = model.EndTime!.Value
        };

    private async Task<RequestListResponse> GetListAsync(string path, CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync(path, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        var payload = await response.Content.ReadFromJsonAsync<RequestListResponse>(cancellationToken: cancellationToken);
        return payload ?? new RequestListResponse();
    }

    private static string BuildQuery(RequestFiltersModel filters)
    {
        var queryParams = new Dictionary<string, string?>
        {
            ["page"] = filters.Page <= 0 ? "1" : filters.Page.ToString(),
            ["pageSize"] = filters.PageSize <= 0 ? "10" : filters.PageSize.ToString(),
            ["search"] = string.IsNullOrWhiteSpace(filters.Search) ? null : filters.Search,
        };

        if (filters.StartDate.HasValue)
            queryParams["startDate"] = filters.StartDate.Value.ToString("yyyy-MM-dd");

        if (filters.EndDate.HasValue)
            queryParams["endDate"] = filters.EndDate.Value.ToString("yyyy-MM-dd");

        if (filters.Status is not RequestStatusFilterOption.All)
            queryParams["status"] = ((int)ConvertToStatus(filters.Status)).ToString();

        queryParams["sortBy"] = ((int)filters.SortBy).ToString();
        queryParams["sortDirection"] = ((int)filters.SortDirection).ToString();

        var builder = new StringBuilder();
        foreach (var pair in queryParams)
        {
            if (string.IsNullOrEmpty(pair.Value))
            {
                continue;
            }

            if (builder.Length == 0)
            {
                builder.Append('?');
            }
            else
            {
                builder.Append('&');
            }

            builder.Append(Uri.EscapeDataString(pair.Key));
            builder.Append('=');
            builder.Append(Uri.EscapeDataString(pair.Value));
        }

        return builder.ToString();
    }

    private static RequestStatusModel ConvertToStatus(RequestStatusFilterOption option) =>
        option switch
        {
            RequestStatusFilterOption.Draft => RequestStatusModel.Draft,
            RequestStatusFilterOption.Pending => RequestStatusModel.Pending,
            RequestStatusFilterOption.Approved => RequestStatusModel.Approved,
            RequestStatusFilterOption.Rejected => RequestStatusModel.Rejected,
            _ => RequestStatusModel.Pending
        };

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var message = await TryGetProblemMessageAsync(response, cancellationToken)
                      ?? response.ReasonPhrase
                      ?? "İşlem tamamlanamadı.";

        throw new InvalidOperationException(message);
    }

    private static async Task<string?> TryGetProblemMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var contentType = response.Content.Headers.ContentType?.MediaType;
            if (contentType is not null && !contentType.Contains("json", StringComparison.OrdinalIgnoreCase))
            {
                var text = await response.Content.ReadAsStringAsync(cancellationToken);
                return string.IsNullOrWhiteSpace(text) ? null : text;
            }

            var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(cancellationToken: cancellationToken);
            if (problem is null)
            {
                return null;
            }

            if (problem.Errors?.Count > 0)
            {
                return string.Join(" ", problem.Errors.SelectMany(kvp => kvp.Value).Distinct());
            }

            return problem.Detail ?? problem.Title;
        }
        catch
        {
            return null;
        }
    }

    private sealed record CreateResponse(Guid Id);
    private sealed record ApprovePayload(string? Reason);
    private sealed record RejectPayload(string Reason);

    private sealed class ValidationProblemDetails
    {
        public string? Title { get; set; }
        public string? Detail { get; set; }
        public Dictionary<string, string[]> Errors { get; set; } = new();
    }

    private sealed record RequestCreateDto
    {
        public Guid BranchId { get; init; }
        public string Title { get; init; } = string.Empty;
        public string? Description { get; init; }
        public DateOnly RequestDate { get; init; }
        public TimeOnly StartTime { get; init; }
        public TimeOnly EndTime { get; init; }
    }
}
