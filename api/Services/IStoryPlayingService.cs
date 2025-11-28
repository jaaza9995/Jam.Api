using Jam.Api.Models;

namespace Jam.Api.Services;

public interface IStoryPlayingService
{
    Task<PlayingSession?> BeginPlayingSessionAsync(Story story, string userId);
    IEnumerable<AnswerOption> FilterAnswerOptionsByLevelAsync(IEnumerable<AnswerOption> allOptions, int level);
    Task<QuestionScene?> GetFirstQuestionSceneAsync(int storyId);
    int GetPointsForCorrectAnswerAsync(int level);
    Task<EndingScene?> GetEndingSceneAsync(int storyId, int score, int maxScore);
}