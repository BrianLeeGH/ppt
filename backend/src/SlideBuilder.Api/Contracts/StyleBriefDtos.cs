namespace SlideBuilder.Api.Contracts;

public record StyleBriefDto(
    Guid Id,
    Guid DeckId,
    string Status,
    StyleBriefFieldsDto Fields,
    DateTime CreatedAt);

public record StyleBriefFieldsDto(
    string VisualTone,
    string AudienceContext,
    string Density,
    string Emphasis,
    string Contrast,
    string TypographyHierarchy,
    string LayoutRhythm,
    string MotionFeel,
    string MediaTreatment,
    string BrandConstraints);

public record UpdateStyleBriefRequest(StyleBriefFieldsDto Fields);

public record StyleImpactSummaryDto(string Summary, bool HasSignificantChanges);
