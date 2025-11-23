
using Jam.Models.Enums;

namespace Jam.DTOs.StoryPlaying;

public class AnswerFeedbackDto
{
    public string? SceneText { get; set; }
    public int NewScore { get; set; }
    public int NewLevel { get; set; }

}