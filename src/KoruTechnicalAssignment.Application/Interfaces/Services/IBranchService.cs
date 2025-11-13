using KoruTechnicalAssignment.Application.DTO;

namespace KoruTechnicalAssignment.Application.Interfaces.Services {
    public interface IBranchService {
        Task<IReadOnlyList<BranchDto>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<BranchDto?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);
    }
}
