using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class CreateReadmdFileService : ICreateReadmdFileService
{
    private readonly AppDbContext _db;
    private readonly ILogger<CreateReadmdFileService> _logger;

    public CreateReadmdFileService(AppDbContext db, ILogger<CreateReadmdFileService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IEnumerable<CreateReadmdFileDto>> GetAllAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching all CreateReadmdFile items");
        // TODO: Map from Entity to DTO using AutoMapper or manual mapping
        // Follow existing mapping patterns in the codebase
        throw new NotImplementedException();
    }

    public async Task<CreateReadmdFileDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        // TODO: Implement
        throw new NotImplementedException();
    }

    public async Task<CreateReadmdFileDto> CreateAsync(CreateCreateReadmdFileRequest request, CancellationToken ct = default)
    {
        // TODO: Validate, create entity, save, return DTO
        throw new NotImplementedException();
    }

    public async Task<CreateReadmdFileDto> UpdateAsync(Guid id, UpdateCreateReadmdFileRequest request, CancellationToken ct = default)
    {
        // TODO: Find entity, update fields, save, return DTO
        throw new NotImplementedException();
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        // TODO: Find entity, remove, save
        throw new NotImplementedException();
    }
}
