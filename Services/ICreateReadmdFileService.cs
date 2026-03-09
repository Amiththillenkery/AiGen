namespace Api.Services;



public interface ICreateReadmdFileService
{
    Task<IEnumerable<CreateReadmdFileDto>> GetAllAsync(CancellationToken ct = default);
    Task<CreateReadmdFileDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<CreateReadmdFileDto> CreateAsync(CreateCreateReadmdFileRequest request, CancellationToken ct = default);
    Task<CreateReadmdFileDto> UpdateAsync(Guid id, UpdateCreateReadmdFileRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
