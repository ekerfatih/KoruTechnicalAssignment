using KoruTechnicalAssignment.Application.DTO;
using KoruTechnicalAssignment.Application.Interfaces.Repositories;
using KoruTechnicalAssignment.Application.Interfaces.Services;

namespace KoruTechnicalAssignment.Application.Services;

public sealed class BranchService : IBranchService {
    private readonly IBranchRepository branches;

    public BranchService(IBranchRepository branches) => this.branches = branches;

    public async Task<IReadOnlyList<BranchDto>> GetAllAsync(CancellationToken ct = default) {
        var entities = await branches.GetAllAsync(ct);
        return entities
            .Select(b => new BranchDto { Id = b.Id, Name = b.Name, Location = b.Location })
            .ToList();
    }

    public async Task<BranchDto?> GetByIdAsync(Guid id, CancellationToken ct = default) {
        var entity = await branches.GetByIdAsync(id, ct);
        return entity is null
            ? null
            : new BranchDto { Id = entity.Id, Name = entity.Name, Location = entity.Location };
    }
}
