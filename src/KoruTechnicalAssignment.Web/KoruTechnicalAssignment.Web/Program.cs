using FluentValidation;
using KoruTechnicalAssignment.Application.DTO;
using KoruTechnicalAssignment.Application.Interfaces.Services;
using KoruTechnicalAssignment.Application.Validators;
using KoruTechnicalAssignment.Domain.Entities.Enums;
using KoruTechnicalAssignment.Domain.Entities.Identity;
using KoruTechnicalAssignment.Infrastructure;
using KoruTechnicalAssignment.Infrastructure.Persistence;
using KoruTechnicalAssignment.Infrastructure.Seed;
using KoruTechnicalAssignment.Web.Client.Services;
using KoruTechnicalAssignment.Web.Components;
using KoruTechnicalAssignment.Web.Components.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options => {
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
.AddIdentityCookies();

builder.Services.AddAuthorizationBuilder();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddValidatorsFromAssemblyContaining<RequestCreateDtoValidator>();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
builder.Services.AddHttpClient<RequestApiClient>(client => {
    var baseUrl = builder.Configuration.GetValue<string>("AppBaseUrl")
                  ?? builder.Configuration.GetValue<string>("ASPNETCORE_URLS")?.Split(';', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()
                  ?? "https://localhost:7122/";
    client.BaseAddress = new Uri(baseUrl);
});
builder.Services.AddScoped<NotificationService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope()) {
    var sp = scope.ServiceProvider;

    var identityDb = sp.GetRequiredService<ApplicationIdentityDbContext>();
    await identityDb.Database.MigrateAsync();
    await IdentitySeed.IdentitySeedAsync(sp);

    var appDb = sp.GetRequiredService<ApplicationDbContext>();
    await appDb.Database.MigrateAsync();
    await DataSeed.SeedAsync(sp);
}

app.Use(async (context, next) => {
    try {
        await next();
    } catch (Exception ex) when (context.Request.Path.StartsWithSegments("/api")) {
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("ApiException");
        logger.LogError(ex, "Unhandled API exception for {Path}", context.Request.Path);

        var problem = new ProblemDetails {
            Title = "Beklenmeyen hata oluştu.",
            Detail = app.Environment.IsDevelopment() ? ex.Message : "İşlem sırasında bir hata oluştu.",
            Status = StatusCodes.Status500InternalServerError,
            Instance = context.Request.Path
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(problem);
    }
});

if (app.Environment.IsDevelopment()) {
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
} else {
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(KoruTechnicalAssignment.Web.Client._Imports).Assembly);

app.MapAdditionalIdentityEndpoints();


var api = app.MapGroup("/api");

var branchGroup = api.MapGroup("/branches");
branchGroup.MapGet("/", async (IBranchService service, CancellationToken ct) => {
    var branches = await service.GetAllAsync(ct);
    return Results.Ok(branches);
});
branchGroup.MapGet("/{id:guid}", async (Guid id, IBranchService service, CancellationToken ct) => {
    var branch = await service.GetByIdAsync(id, ct);
    return branch is null ? Results.NotFound() : Results.Ok(branch);
});

var requestGroup = api.MapGroup("/requests").RequireAuthorization();

requestGroup.MapGet("/", async (
    [FromQuery] RequestStatus? status,
    [FromQuery] DateOnly? startDate,
    [FromQuery] DateOnly? endDate,
    [FromQuery] string? search,
    [FromQuery] int page,
    [FromQuery] int pageSize,
    [FromQuery] RequestSortField? sortBy,
    [FromQuery] SortDirection? sortDirection,
    ClaimsPrincipal user,
    IRequestService service,
    CancellationToken ct) => {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var sortField = sortBy ?? RequestSortField.Date;
        var sortDir = sortDirection ?? SortDirection.Desc;

        var (items, total) = await service.GetUserRequestsAsync(
            userId, status, startDate, endDate, search, page, pageSize, sortField, sortDir, ct);

        return Results.Ok(new { total, items });
    });

requestGroup.MapGet("/{id:guid}", async (
    Guid id,
    ClaimsPrincipal user,
    IRequestService service,
    CancellationToken ct) => {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var request = await service.GetByIdForUserAsync(id, userId, ct);
        return request is null ? Results.NotFound() : Results.Ok(request);
    });

requestGroup.MapPost("/", async (
    RequestCreateDto dto,
    ClaimsPrincipal user,
    IRequestService service,
    IValidator<RequestCreateDto> validator,
    CancellationToken ct) => {
        var validation = await validator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return ValidationProblem(validation);

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var id = await service.CreateDraftAsync(dto, userId, ct);
        return Results.Created($"/api/requests/{id}", new { id });
    });

requestGroup.MapPut("/{id:guid}", async (
    Guid id,
    RequestUpdateDto dto,
    ClaimsPrincipal user,
    IRequestService service,
    IValidator<RequestUpdateDto> validator,
    CancellationToken ct) => {
        var validation = await validator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return ValidationProblem(validation);

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await service.UpdateDraftAsync(id, dto, userId, ct);
        return Results.NoContent();
    });

requestGroup.MapPost("/{id:guid}/submit", async (
    Guid id,
    ClaimsPrincipal user,
    IRequestService service,
    CancellationToken ct) => {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await service.SubmitAsync(id, userId, ct);
        return Results.NoContent();
    });

var adminGroup = requestGroup
    .MapGroup("/admin")
    .RequireAuthorization(policy => policy.RequireRole("Admin"));

adminGroup.MapGet("/", async (
    [FromQuery] DateOnly? startDate,
    [FromQuery] DateOnly? endDate,
    [FromQuery] string? search,
    [FromQuery] int page,
    [FromQuery] int pageSize,
    [FromQuery] RequestSortField? sortBy,
    [FromQuery] SortDirection? sortDirection,
    IRequestService service,
    CancellationToken ct) => {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var sortField = sortBy ?? RequestSortField.Date;
        var sortDir = sortDirection ?? SortDirection.Desc;

        var (items, total) = await service.GetPendingRequestsAsync(startDate, endDate, search, page, pageSize, sortField, sortDir, ct);
        return Results.Ok(new { total, items });
    });

adminGroup.MapGet("/{id:guid}", async (
    Guid id,
    IRequestService service,
    CancellationToken ct) => {
        var request = await service.GetByIdForAdminAsync(id, ct);
        return request is null ? Results.NotFound() : Results.Ok(request);
    });

adminGroup.MapPost("/{id:guid}/approve", async (
    Guid id,
    RequestApproveDto dto,
    ClaimsPrincipal user,
    IRequestService service,
    IValidator<RequestApproveDto> validator,
    CancellationToken ct) => {
        var validation = await validator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return ValidationProblem(validation);

        var adminId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await service.ApproveAsync(id, adminId, dto.Reason, ct);
        return Results.NoContent();
    });

adminGroup.MapPost("/{id:guid}/reject", async (
    Guid id,
    RequestRejectDto dto,
    ClaimsPrincipal user,
    IRequestService service,
    IValidator<RequestRejectDto> validator,
    CancellationToken ct) => {
        var validation = await validator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return ValidationProblem(validation);

        var adminId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await service.RejectAsync(id, adminId, dto.Reason, ct);
        return Results.NoContent();
    });

static IResult ValidationProblem(FluentValidation.Results.ValidationResult validationResult) {
    var errors = validationResult.Errors
        .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
        .ToDictionary(g => g.Key, g => g.ToArray());

    return Results.ValidationProblem(errors);
}

app.Run();

