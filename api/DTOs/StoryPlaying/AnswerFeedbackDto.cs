using Jam.Api.Models.Enums;

namespace Jam.Api.DTOs.StoryPlaying;

public class AnswerFeedbackDto
{
    public string SceneText { get; set; } = string.Empty;
    public int SessionId { get; set; }
    public int StoryId { get; set; }
    public int? NextSceneId { get; set; }
    public SceneType NextSceneType { get; set; }
    public int NewScore { get; set; }
    public int NewLevel { get; set; }
    public int SelectedAnswerId { get; set; }
    public bool IsGameOver { get; set; } = false;
    public string? Message { get; set; }
}
