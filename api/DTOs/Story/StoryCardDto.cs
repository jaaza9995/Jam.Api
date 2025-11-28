using Jam.Api.Models.Enums;

namespace Jam.Api.DTOs.Story;

public class StoryCardDto
{
    public int StoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int QuestionCount { get; set; }
    public DifficultyLevel DifficultyLevel { get; set; }
    public Accessibility Accessibility { get; set; }
    public string Code { get; set; } = string.Empty;
}