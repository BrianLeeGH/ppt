using Microsoft.EntityFrameworkCore;
using SlideBuilder.Core.Domain;

namespace SlideBuilder.Infrastructure.Persistence;

public class SlideBuilderDbContext : DbContext
{
    public SlideBuilderDbContext(DbContextOptions<SlideBuilderDbContext> options)
        : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Deck> Decks => Set<Deck>();
    public DbSet<Outline> Outlines => Set<Outline>();
    public DbSet<OutlineRevision> OutlineRevisions => Set<OutlineRevision>();
    public DbSet<StyleBrief> StyleBriefs => Set<StyleBrief>();
    public DbSet<Slide> Slides => Set<Slide>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<ConversationMessage> Messages => Set<ConversationMessage>();
    public DbSet<ConversationSummary> ConversationSummaries => Set<ConversationSummary>();
    public DbSet<Revision> Revisions => Set<Revision>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<Checkpoint> Checkpoints => Set<Checkpoint>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Deck)
                  .WithOne()
                  .HasForeignKey<Deck>(e => e.ProjectId);
        });

        modelBuilder.Entity<Deck>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.DraftOutline)
                  .WithOne()
                  .HasForeignKey<Outline>(e => e.DeckId);
            entity.HasOne(e => e.DraftStyleBrief)
                  .WithOne()
                  .HasForeignKey<StyleBrief>(e => e.DeckId);
        });

        modelBuilder.Entity<Outline>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Revisions)
                  .WithOne(r => r.Outline)
                  .HasForeignKey(r => r.OutlineId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OutlineRevision>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.OutlineId, e.RevisionNumber })
                  .HasDatabaseName("IX_OutlineRevisions_OutlineId_RevisionNumber");
            entity.HasOne(e => e.TriggeredByMessage)
                  .WithMany()
                  .HasForeignKey(e => e.TriggeredByMessageId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<StyleBrief>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<Slide>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<Asset>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<ConversationMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ProjectId, e.CreatedAt })
                  .HasDatabaseName("IX_ConversationMessages_ProjectId_CreatedAt");
        });

        modelBuilder.Entity<ConversationSummary>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ProjectId, e.CoveringMessagesFrom })
                  .HasDatabaseName("IX_ConversationSummaries_ProjectId_Range");
            entity.HasOne(e => e.Project)
                  .WithMany(p => p.ConversationSummaries)
                  .HasForeignKey(e => e.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Revision>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<Checkpoint>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
    }
}
