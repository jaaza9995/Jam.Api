using Jam.Api.Models.Enums;
using Jam.Api.DTOs.QuestionScenes;

namespace Jam.Api.DTOs.Story;

public class StoryCreationSession
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public DifficultyLevel DifficultyLevel { get; set; }
    public Accessibility Accessibility { get; set; }

    public string IntroText { get; set; } = "";

    public List<QuestionSceneDto> QuestionScenes { get; set; } = new();

    public string GoodEnding { get; set; } = "";
    public string NeutralEnding { get; set; } = "";
    public string BadEnding { get; set; } = "";
}