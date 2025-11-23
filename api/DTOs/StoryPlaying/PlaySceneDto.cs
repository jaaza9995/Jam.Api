using Jam.Models.Enums;

namespace Jam.DTOs.StoryPlaying
{
    public class PlaySceneDto
    {
        public int SessionId { get; set; }
        public int SceneId { get; set; }
        public string SceneText { get; set; } = string.Empty;
        public SceneType SceneType { get; set; }

        public string? Question { get; set; }
        public List<PlayAnswerOptionDto>? AnswerOptions { get; set; }

        public int CurrentScore { get; set; }
        public int MaxScore { get; set; }
        public int CurrentLevel { get; set; }

        public int? NextSceneAfterIntroId { get; set; }
    }

    public class PlayAnswerOptionDto
    {
        public int AnswerOptionId { get; set; }
        public string AnswerText { get; set; } = string.Empty;
    }
}
