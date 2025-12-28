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
    public DbSet<StyleBrief> StyleBriefs => Set<StyleBrief>();
    public DbSet<Slide> Slides => Set<Slide>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<ConversationMessage> Messages => Set<ConversationMessage>();
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
