using SlideBuilder.Core.Domain;
using System.Text;
using System.Text.Json;

namespace SlideBuilder.Core.AI.Prompts;

public class OutlinePromptBuilder
{
    public string BuildInitialGenerationPrompt(string userDescription, Outline? currentOutline = null)
    {
        var prompt = new StringBuilder();

        prompt.AppendLine("You are an expert presentation outline creator. Your task is to generate a structured presentation outline based on the user's description.");
        prompt.AppendLine();
        prompt.AppendLine("INSTRUCTIONS:");
        prompt.AppendLine("1. Generate slide titles and 2-5 key points per slide");
        prompt.AppendLine("2. Structure the presentation logically with introduction, main content, and conclusion");
        prompt.AppendLine("3. Ensure key points are concise and action-oriented");
        prompt.AppendLine("4. If the request is ambiguous, set needsClarification to true and provide clarification questions");
        prompt.AppendLine();
        prompt.AppendLine("USER REQUEST:");
        prompt.AppendLine(userDescription);
        prompt.AppendLine();

        if (currentOutline != null)
        {
            prompt.AppendLine("CURRENT OUTLINE:");
            prompt.AppendLine(currentOutline.SlidesJson);
            prompt.AppendLine();
        }

        prompt.AppendLine("OUTPUT FORMAT (JSON):");
        prompt.AppendLine("```json");
        prompt.AppendLine("{");
        prompt.AppendLine("  \"slides\": [");
        prompt.AppendLine("    {");
        prompt.AppendLine("      \"title\": \"Slide Title\",");
        prompt.AppendLine("      \"keyPoints\": [\"Point 1\", \"Point 2\", \"Point 3\"]");
        prompt.AppendLine("    }");
        prompt.AppendLine("  ],");
        prompt.AppendLine("  \"needsClarification\": false,");
        prompt.AppendLine("  \"clarificationQuestions\": []");
        prompt.AppendLine("}");
        prompt.AppendLine("```");

        return prompt.ToString();
    }

    public string BuildEditRequestPrompt(string userRequest, Outline currentOutline)
    {
        var prompt = new StringBuilder();

        prompt.AppendLine("You are an outline editor. Parse the user's edit request and determine the operation to perform.");
        prompt.AppendLine();
        prompt.AppendLine("CURRENT OUTLINE:");
        prompt.AppendLine(currentOutline.SlidesJson);
        prompt.AppendLine();
        prompt.AppendLine("USER EDIT REQUEST:");
        prompt.AppendLine(userRequest);
        prompt.AppendLine();
        prompt.AppendLine("AVAILABLE OPERATIONS:");
        prompt.AppendLine("- Insert: Add a new slide at a specific position");
        prompt.AppendLine("- Delete: Remove a slide");
        prompt.AppendLine("- Reorder: Move slides to different positions");
        prompt.AppendLine("- Modify: Edit slide title or key points");
        prompt.AppendLine("- Approve: User wants to approve the outline (phrases like 'approve', 'looks good', 'confirm')");
        prompt.AppendLine();
        prompt.AppendLine("If the request is to ADD or INSERT a slide, generate the full outline with the new slide included.");
        prompt.AppendLine("If the request is to MODIFY, DELETE, or REORDER, generate the full updated outline.");
        prompt.AppendLine();
        prompt.AppendLine("OUTPUT FORMAT (JSON):");
        prompt.AppendLine("```json");
        prompt.AppendLine("{");
        prompt.AppendLine("  \"slides\": [ /* full updated outline */ ],");
        prompt.AppendLine("  \"needsClarification\": false,");
        prompt.AppendLine("  \"clarificationQuestions\": []");
        prompt.AppendLine("}");
        prompt.AppendLine("```");

        return prompt.ToString();
    }

    public string BuildApprovalPrompt(Outline outline)
    {
        var prompt = new StringBuilder();

        prompt.AppendLine("The user wants to approve this outline. Confirm the approval and explain next steps.");
        prompt.AppendLine();
        prompt.AppendLine("OUTLINE TO APPROVE:");
        prompt.AppendLine(outline.SlidesJson);
        prompt.AppendLine();
        prompt.AppendLine("Respond with a confirmation message that:");
        prompt.AppendLine("1. Confirms the outline is now approved");
        prompt.AppendLine("2. Mentions the outline is locked for slide generation");
        prompt.AppendLine("3. Explains that editing will create a new draft version");
        prompt.AppendLine();
        prompt.AppendLine("Keep the response conversational and friendly (2-3 sentences).");

        return prompt.ToString();
    }

    public bool IsApprovalRequest(string userMessage)
    {
        var approvalKeywords = new[]
        {
            "approve", "approved", "approval", "confirm", "confirmed",
            "looks good", "looks great", "perfect", "done", "ready",
            "proceed", "continue", "next", "go ahead"
        };

        var lowerMessage = userMessage.ToLowerInvariant();
        return approvalKeywords.Any(keyword => lowerMessage.Contains(keyword));
    }

    public bool IsEditRequest(string userMessage, Outline? currentOutline)
    {
        // If there's no outline yet, it's an initial generation request
        if (currentOutline == null)
            return false;

        var editKeywords = new[]
        {
            "add", "insert", "remove", "delete", "change", "modify",
            "update", "edit", "move", "reorder", "swap"
        };

        var lowerMessage = userMessage.ToLowerInvariant();
        return editKeywords.Any(keyword => lowerMessage.Contains(keyword));
    }
}
