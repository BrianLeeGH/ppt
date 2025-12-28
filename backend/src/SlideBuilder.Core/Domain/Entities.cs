namespace SlideBuilder.Core.Domain;

public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Deck? Deck { get; set; }
    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
    public ICollection<ConversationMessage> Messages { get; set; } = new List<ConversationMessage>();
    public ICollection<Job> Jobs { get; set; } = new List<Job>();
    public ICollection<Revision> Revisions { get; set; } = new List<Revision>();
}

public class Deck
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string? Title { get; set; }
    public string? IntentSummary { get; set; }

    public Outline? DraftOutline { get; set; }
    public StyleBrief? DraftStyleBrief { get; set; }
    public ICollection<Slide> Slides { get; set; } = new List<Slide>();
}

public class Outline
{
    public Guid Id { get; set; }
    public Guid DeckId { get; set; }
    public OutlineStatus Status { get; set; }
    public string SlidesJson { get; set; } = "[]"; // Store as JSON for simplicity in MVP
    public DateTime CreatedAt { get; set; }
}

public enum OutlineStatus
{
    Draft,
    Approved
}

public class StyleBrief
{
    public Guid Id { get; set; }
    public Guid DeckId { get; set; }
    public StyleBriefStatus Status { get; set; }
    public string FieldsJson { get; set; } = "{}"; // Store as JSON
    public DateTime CreatedAt { get; set; }
}

public enum StyleBriefStatus
{
    Draft,
    Approved
}

public class Slide
{
    public Guid Id { get; set; }
    public Guid DeckId { get; set; }
    public int OrderIndex { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ContentBlocksJson { get; set; } = "[]";
    public string? SpeakerNotes { get; set; }
}

public class ContentBlock
{
    public string Type { get; set; } = "Text"; // Text, Image
    public string? Content { get; set; }
    public Guid? AssetId { get; set; }
    public string? AssetUrl { get; set; }
}

public class Asset
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Kind { get; set; } = string.Empty; // Image/Other
    public string OriginalName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string StorageKey { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty; // Upload/URL
    public DateTime CreatedAt { get; set; }
}

public class ConversationMessage
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Role { get; set; } = string.Empty; // User/Assistant/System
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class Revision
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Summary { get; set; }

    // Snapshots
    public string OutlineSnapshotJson { get; set; } = string.Empty;
    public string StyleBriefSnapshotJson { get; set; } = string.Empty;
    public string SlidesSnapshotJson { get; set; } = string.Empty;
}

public class Job
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public JobStatus Status { get; set; }
    public string Stage { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? Error { get; set; }
    public string? InputSummary { get; set; }

    public ICollection<Checkpoint> Checkpoints { get; set; } = new List<Checkpoint>();
}

public enum JobStatus
{
    Queued,
    Running,
    Succeeded,
    Failed,
    Stopped
}

public class Checkpoint
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public string StageCompleted { get; set; } = string.Empty;
    public Guid? RevisionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Details { get; set; }
}
