namespace KoruTechnicalAssignment.Application.Interfaces.Services;

public interface IUserProfileService {
    Task<string?> GetEmailAsync(string userId, CancellationToken ct = default);
}
