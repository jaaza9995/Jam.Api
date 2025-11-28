using Jam.Api.Models;
using Jam.Api.Models.Enums;
using Jam.Api.DAL.StoryDAL;
using Jam.Api.DAL.PlayingSessionDAL;
using Jam.Api.DAL.SceneDAL;


namespace Jam.Api.Services;

public class StoryPlayingService : IStoryPlayingService
{
    private readonly IPlayingSessionRepository _playingSessionRepository;
    private readonly IStoryRepository _storyRepository;
    private readonly ISceneRepository _sceneRepository;
    private readonly ILogger<StoryPlayingService> _logger;


    public StoryPlayingService(
        IPlayingSessionRepository playingSessionRepository,
        IStoryRepository storyRepository,
        ISceneRepository sceneRepository,
        ILogger<StoryPlayingService> logger
        )
    {
        _playingSessionRepository = playingSessionRepository;
        _storyRepository = storyRepository;
        _sceneRepository = sceneRepository;
        _logger = logger;
    }

    public async Task<PlayingSession?> BeginPlayingSessionAsync(Story story, string userId)
    {
        await _storyRepository.IncrementPlayed(story.StoryId);

        var introScene = await _sceneRepository.GetIntroSceneByStoryId(story.StoryId);

        if (introScene == null)
        {
            _logger.LogError("[StoryPlayingService -> BeginPlayingSessionAsync] Story {StoryId} has no IntroScene defined.", story.StoryId);
            return null;
        }

        var amountOfQuestions = await _storyRepository.GetAmountOfQuestionsForStory(story.StoryId) ?? 0;
        var maxScore = Math.Max(amountOfQuestions * 5, 0);

        var session = new PlayingSession
        {
            StartTime = DateTime.UtcNow,
            Score = 0,
            MaxScore = maxScore,
            CurrentLevel = 3,
            CurrentSceneId = introScene.IntroSceneId,
            CurrentSceneType = SceneType.Intro,
            StoryId = story.StoryId,
            UserId = userId
        };

        await _playingSessionRepository.AddPlayingSession(session);
        return session;
    }

    public IEnumerable<AnswerOption> FilterAnswerOptionsByLevelAsync(IEnumerable<AnswerOption> allOptions, int level)
    {
        var options = allOptions.ToList();
        var correct = options.FirstOrDefault(a => a.IsCorrect);
        var wrong = options.Where(a => !a.IsCorrect).ToList();
        var random = new Random();
        int count = level switch { 3 => 4, 2 => 3, 1 => 2, _ => 4 };
        var selected = new List<AnswerOption> { correct! };
        selected.AddRange(wrong.OrderBy(_ => random.Next()).Take(count - 1));
        return selected.OrderBy(_ => random.Next());
    }

    public async Task<QuestionScene?> GetFirstQuestionSceneAsync(int storyId)
    {
        var firstQuestionScene = await _sceneRepository.GetFirstQuestionSceneByStoryId(storyId);

        if (firstQuestionScene != null)
        {
            return firstQuestionScene;
        }
        else
        {
            _logger.LogWarning("[StoryPlayingService -> GetFirstQuestionSceneAsync] Story {StoryId} has IntroScene but no QuestionScenes.", storyId);
            return null;
        }
    }

    public int GetPointsForCorrectAnswerAsync(int level)
    {
        return level switch
        {
            3 => 5, // 5 points for answering question correctly at level 3
            2 => 3, // 3 points for answering question correctly at level 2
            1 => 1, // 1 point for answering question correctly at level 1
            _ => 0
        };
    }

    public async Task<EndingScene?> GetEndingSceneAsync(int storyId, int score, int maxScore)
    {
        if (maxScore <= 0)
        {
            _logger.LogError("[StoryPlayingService -> GetEndingSceneAsync] Max score for story {StoryId} is non-positive", storyId);
            return null;
        }

        double percentage = (double)score / maxScore * 100;

        EndingScene? endingScene = percentage switch
        {
            >= 70 => await _sceneRepository.GetGoodEndingSceneByStoryId(storyId),
            >= 20 => await _sceneRepository.GetNeutralEndingSceneByStoryId(storyId),
            _ => await _sceneRepository.GetBadEndingSceneByStoryId(storyId)
        };

        if (endingScene == null)
        {
            _logger.LogError("No ending scene found for story {StoryId}", storyId);
        }

        return endingScene;
    }
}