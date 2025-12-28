using SlideBuilder.Core.Domain;

namespace SlideBuilder.Core.Guards;

public static class OutlineApprovalGuard
{
    public static void EnsureApproved(Outline? outline)
    {
        if (outline == null)
        {
            throw new InvalidOperationException("Outline not found.");
        }

        if (outline.Status != OutlineStatus.Approved)
        {
            throw new InvalidOperationException("Outline must be approved before proceeding.");
        }
    }
}
