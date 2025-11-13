using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KoruTechnicalAssignment.Web.Client.Models;

public enum RequestStatusModel
{
    Draft = 0,
    Pending = 1,
    Approved = 2,
    Rejected = 3
}

public sealed class BranchModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}

public sealed class RequestListItemModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string RequesterName { get; set; } = string.Empty;
    public DateOnly RequestDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public RequestStatusModel Status { get; set; }
}

public sealed class RequestStatusHistoryModel
{
    public Guid Id { get; set; }
    public RequestStatusModel Status { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime ChangedAt { get; set; }
}

public sealed class RequestDetailModel
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string RequesterName { get; set; } = string.Empty;
    public DateOnly RequestDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public RequestStatusModel Status { get; set; }
    public List<RequestStatusHistoryModel> History { get; set; } = [];
}

public sealed class RequestListResponse
{
    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("items")]
    public List<RequestListItemModel> Items { get; set; } = [];
}

public sealed class RequestFiltersModel
{
    public RequestStatusFilterOption Status { get; set; } = RequestStatusFilterOption.All;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public RequestSortField SortBy { get; set; } = RequestSortField.Date;
    public SortDirection SortDirection { get; set; } = SortDirection.Desc;
}

public enum RequestStatusFilterOption
{
    All,
    Draft,
    Pending,
    Approved,
    Rejected
}

public sealed class RequestFormModel
{
    [Required(ErrorMessage = "Sube secimi zorunludur.")]
    public Guid? BranchId { get; set; }

    [Required, StringLength(200, ErrorMessage = "Baslik en fazla 200 karakter olabilir.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Talep tarihi zorunludur.")]
    public DateOnly? RequestDate { get; set; }

    [Required(ErrorMessage = "Baslangic saati zorunludur.")]
    public TimeOnly? StartTime { get; set; }

    [Required(ErrorMessage = "Bitis saati zorunludur.")]
    public TimeOnly? EndTime { get; set; }
}

public static class RequestStatusDisplay
{
    public static string GetFilterText(RequestStatusFilterOption option) =>
        option switch
        {
            RequestStatusFilterOption.All => "Tümü",
            RequestStatusFilterOption.Draft => "Taslak",
            RequestStatusFilterOption.Pending => "Beklemede",
            RequestStatusFilterOption.Approved => "Onaylandı",
            RequestStatusFilterOption.Rejected => "Reddedildi",
            _ => option.ToString()
        };

    public static string GetStatusText(RequestStatusModel status) =>
        status switch
        {
            RequestStatusModel.Draft => "Taslak",
            RequestStatusModel.Pending => "Beklemede",
            RequestStatusModel.Approved => "Onaylandı",
            RequestStatusModel.Rejected => "Reddedildi",
            _ => status.ToString()
        };

    public static string GetBadgeClass(RequestStatusModel status) =>
        status switch
        {
            RequestStatusModel.Approved => "badge bg-success",
            RequestStatusModel.Pending => "badge bg-warning text-dark",
            RequestStatusModel.Rejected => "badge bg-danger",
            _ => "badge bg-secondary"
        };
}

public enum RequestSortField
{
    Date = 0,
    Status = 1
}

public enum SortDirection
{
    Asc = 0,
    Desc = 1
}
