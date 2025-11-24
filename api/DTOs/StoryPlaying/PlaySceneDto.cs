using Jam.Models;
using Jam.Models.Enums;

namespace Jam.Api.DTOs.StoryPlaying;

public class PlaySceneDto
{
    // Common to all types of scenes
    public int SessionId { get; set; }
    public int SceneId { get; set; }
    public string SceneText { get; set; } = string.Empty;
    public SceneType SceneType { get; set; }


    // For QuestionScenes
    public string? Question { get; set; }
    public IEnumerable<AnswerOption>? AnswerOptions { get; set; } 
    public int? SelectedAnswerId { get; set; } // user’s selected answer in the form 
    public Guid AnswerRandomSeed { get; set; } // in case client-side validation fails 


    public int? NextSceneAfterIntroId { get; set; } // To navigate from IntroScene to first QuestionScene


    // Playing stats
    public int CurrentScore { get; set; }
    public int MaxScore { get; set; }
    public int CurrentLevel { get; set; }
}