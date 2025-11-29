using Jam.Api.Models;
using Jam.Api.Models.Enums;
using Jam.Api.DTOs.StoryPlaying;

namespace Jam.Api.Services;

public interface IStoryPlayingService
{
    Task<PlayingSession?> BeginPlayingSessionAsync(Story story, string userId);
    Task<PlaySceneDto?> BuildPlaySceneDtoAsync(int sceneId, SceneType sceneType, int sessionId);
    Task<bool> TransitionFromIntroToFirstQuestionAsync(int sessionId, int sceneId);
    Task<AnswerFeedbackDto> ProcessAnswerAsync(int selectedAnswerId, int sessionId);
    Task<EndingScene?> GetEndingSceneAsync(int storyId, int score, int maxScore);
}