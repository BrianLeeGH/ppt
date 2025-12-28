using Microsoft.EntityFrameworkCore;
using SlideBuilder.Core.Domain;
using SlideBuilder.Core.Persistence;

namespace SlideBuilder.Infrastructure.Persistence.Repositories;

public class OutlineRevisionRepository : Repository<OutlineRevision>, IOutlineRevisionRepository
{
    public OutlineRevisionRepository(SlideBuilderDbContext context) : base(context)
    {
    }

    public async Task<List<OutlineRevision>> GetByOutlineIdAsync(Guid outlineId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.OutlineId == outlineId)
            .OrderBy(r => r.RevisionNumber)
            .ToListAsync(cancellationToken);
    }
}
