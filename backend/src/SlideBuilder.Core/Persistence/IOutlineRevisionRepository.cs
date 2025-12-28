using SlideBuilder.Core.Domain;

namespace SlideBuilder.Core.Persistence;

public interface IOutlineRevisionRepository : IRepository<OutlineRevision>
{
    Task<List<OutlineRevision>> GetByOutlineIdAsync(Guid outlineId, CancellationToken cancellationToken = default);
}
