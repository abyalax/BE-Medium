using Medium.Api.Infrastructure.Database;

using Microsoft.EntityFrameworkCore;

using ObjectStorageModel = Medium.Api.Models.ObjectStorage;

namespace Medium.Api.Infrastructure.Storage.Repositories;

public class ObjectStorageRepository(ApplicationDbContext context)
{
  private readonly ApplicationDbContext _context = context;

  public async Task AddAsync(ObjectStorageModel objectStorage, CancellationToken cancellationToken = default)
  {
    await _context.ObjectStorages.AddAsync(objectStorage, cancellationToken);
  }

  public async Task<ObjectStorageModel?> GetByKeyAsync(string objectKey, CancellationToken cancellationToken = default)
  {
    return await _context.ObjectStorages
        .FirstOrDefaultAsync(x => x.ObjectKey == objectKey, cancellationToken);
  }

  public async Task<ObjectStorageModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return await _context.ObjectStorages
        .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
  }

  public async Task<List<ObjectStorageModel>> GetByAuthorAsync(Guid authorId, CancellationToken cancellationToken = default)
  {
    return await _context.ObjectStorages
        .Where(x => x.AuthorId == authorId)
        .OrderByDescending(x => x.CreatedAt)
        .ToListAsync(cancellationToken);
  }

  public void Delete(ObjectStorageModel objectStorage)
  {
    _context.ObjectStorages.Remove(objectStorage);
  }

  public void DeleteRange(List<ObjectStorageModel> objectStorages)
  {
    _context.ObjectStorages.RemoveRange(objectStorages);
  }

  public async Task SaveAsync(CancellationToken cancellationToken = default)
  {
    await _context.SaveChangesAsync(cancellationToken);
  }
}