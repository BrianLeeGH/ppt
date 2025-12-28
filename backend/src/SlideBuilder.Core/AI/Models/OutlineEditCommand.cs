namespace SlideBuilder.Core.AI.Models;

public class OutlineEditCommand
{
    public EditOperation Operation { get; set; }
    public int? TargetSlideIndex { get; set; }
    public SlideProposal? NewSlide { get; set; }
    public string? ModificationInstructions { get; set; }
    public bool RequiresConfirmation { get; set; }
    public string? ConfirmationMessage { get; set; }
}

public enum EditOperation
{
    Insert,
    Delete,
    Reorder,
    Modify,
    Approve
}
