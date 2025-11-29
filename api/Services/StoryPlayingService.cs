using Jam.Api.Models;
using Jam.Api.Models.Enums;
using Jam.Api.DAL.StoryDAL;
using Jam.Api.DAL.PlayingSessionDAL;
using Jam.Api.DAL.SceneDAL;
using Jam.Api.DTOs.StoryPlaying;
using Jam.Api.DAL.AnswerOptionDAL;


namespace Jam.Api.Services;

public class StoryPlayingService : IStoryPlayingService
{
    private readonly IPlayingSessionRepository _playingSessionRepository;
    private readonly IStoryRepository _storyRepository;
    private readonly ISceneRepository _sceneRepository;
    private readonly IAnswerOptionRepository _answerOptionRepository;
    private readonly ILogger<StoryPlayingService> _logger;


    public StoryPlayingService(
        IPlayingSessionRepository playingSessionRepository,
        IStoryRepository storyRepository,
        ISceneRepository sceneRepository,
        IAnswerOptionRepository answerOptionRepository,
        ILogger<StoryPlayingService> logger
    )
    {
        _playingSessionRepository = playingSessionRepository;
        _storyRepository = storyRepository;
        _sceneRepository = sceneRepository;
        _answerOptionRepository = answerOptionRepository;
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

    public async Task<PlaySceneDto?> BuildPlaySceneDtoAsync(int sceneId, SceneType sceneType, int sessionId)
    {
        object? scene = sceneType switch
        {
            SceneType.Intro => await _sceneRepository.GetIntroSceneById(sceneId),
            SceneType.Question => await _sceneRepository.GetQuestionSceneWithAnswerOptionsById(sceneId),
            SceneType.Ending => await _sceneRepository.GetEndingSceneById(sceneId),
            _ => null
        };

        if (scene == null) return null;

        var session = await _playingSessionRepository.GetPlayingSessionById(sessionId);
        if (session == null) return null;

        var dto = new PlaySceneDto
        {
            SessionId = sessionId,
            SceneId = sceneId,
            SceneType = sceneType,
            SceneText = sceneType switch
            {
                SceneType.Intro => ((IntroScene)scene).IntroText,
                SceneType.Question => ((QuestionScene)scene).SceneText,
                SceneType.Ending => ((EndingScene)scene).EndingText,
                _ => string.Empty
            },
            Question = sceneType == SceneType.Question ? ((QuestionScene)scene).Question : null,
            AnswerOptions = sceneType == SceneType.Question
                ? FilterAnswerOptionsByLevel(((QuestionScene)scene).AnswerOptions, session.CurrentLevel)
                : null,
            NextSceneAfterIntroId = null,
            CurrentScore = session.Score,
            MaxScore = session.MaxScore,
            CurrentLevel = session.CurrentLevel
        };

        if (sceneType == SceneType.Intro)
        {
            var firstQuestionScene = await GetFirstQuestionSceneAsync(session.StoryId);
            dto.NextSceneAfterIntroId = firstQuestionScene?.QuestionSceneId;
        }

        return dto;
    }


    private IEnumerable<AnswerOption> FilterAnswerOptionsByLevel(IEnumerable<AnswerOption> allOptions, int level)
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

    public async Task<bool> TransitionFromIntroToFirstQuestionAsync(int sessionId, int sceneId)
    {
        var session = await _playingSessionRepository.GetPlayingSessionById(sessionId);
        if (session?.CurrentSceneType != SceneType.Intro)
            return false;

        return await _playingSessionRepository.TransitionFromIntroToFirstQuestion(
            sessionId,
            sceneId,
            SceneType.Question
        );
    }

    private async Task<QuestionScene?> GetFirstQuestionSceneAsync(int storyId)
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

    public async Task<AnswerFeedbackDto> ProcessAnswerAsync(int selectedAnswerId, int sessionId)
    {
        var selectedAnswer = await _answerOptionRepository.GetAnswerOptionById(selectedAnswerId);
        if (selectedAnswer == null) throw new ArgumentException("Answer not found");

        var session = await _playingSessionRepository.GetPlayingSessionById(sessionId);
        if (session == null) throw new ArgumentException("Session not found");

        bool isCorrect = selectedAnswer.IsCorrect;
        int points = isCorrect ? GetPointsForCorrectAnswer(session.CurrentLevel) : 0;
        int newScore = session.Score + points;
        int newLevel = isCorrect
            ? Math.Min(session.CurrentLevel + 1, 3)
            : Math.Max(session.CurrentLevel - 1, 1);

        // Game Over (Level 1 Fail)
        if (!isCorrect && session.CurrentLevel == 1)
        {
            await _playingSessionRepository.FinishSession(sessionId, newScore, newLevel);
            await _storyRepository.IncrementFailed(session.StoryId);
            return new AnswerFeedbackDto
            {
                SceneText = selectedAnswer.FeedbackText,
                NewScore = newScore,
                NewLevel = newLevel,
                NextSceneId = null,
                NextSceneType = SceneType.Ending,
                IsGameOver = true,
                Message = "Game over"
            };
        }

        // Get next scene
        int currentQuestionSceneId = session.CurrentSceneId.GetValueOrDefault();
        var nextScene = await _sceneRepository.GetNextQuestionSceneById(currentQuestionSceneId);

        int? nextSceneId = nextScene?.QuestionSceneId; // can be null if it is the last QuestionScene
        SceneType nextSceneType = nextScene != null ? SceneType.Question : SceneType.Ending;

        // Last QuestionScene -> EndingScene
        if (!nextSceneId.HasValue)
        {
            var finalEndingScene = await GetEndingSceneAsync(session.StoryId, newScore, session.MaxScore);
            if (finalEndingScene == null)
                throw new InvalidOperationException("Could not find appropriate ending scene");

            nextSceneId = finalEndingScene.EndingSceneId;
            nextSceneType = SceneType.Ending;
            await _storyRepository.IncrementFinished(session.StoryId);
        }

        // Update session progress
        await _playingSessionRepository.AnswerQuestion(sessionId, nextSceneId, nextSceneType, newScore, newLevel);

        return new AnswerFeedbackDto
        {
            SceneText = selectedAnswer.FeedbackText,
            NewScore = newScore,
            NewLevel = newLevel,
            NextSceneId = nextSceneId,
            NextSceneType = nextSceneType
        };
    }


    private int GetPointsForCorrectAnswer(int level)
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