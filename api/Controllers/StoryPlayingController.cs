using Jam.DAL.AnswerOptionDAL;
using Jam.DAL.PlayingSessionDAL;
using Jam.DAL.SceneDAL;
using Jam.DAL.StoryDAL;
using Jam.DTOs;
using Jam.Models;
using Jam.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Jam.DTOs.StoryPlaying;

namespace Jam.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoryPlayingController : ControllerBase
{
    private readonly IPlayingSessionRepository _playingSessionRepository;
    private readonly IStoryRepository _storyRepository;
    private readonly ISceneRepository _sceneRepository;
    private readonly IAnswerOptionRepository _answerOptionRepository;
    private readonly UserManager<AuthUser> _userManager;
    private readonly ILogger<StoryPlayingController> _logger;

    public StoryPlayingController(
        IPlayingSessionRepository playingSessionRepository,
        IStoryRepository storyRepository,
        ISceneRepository sceneRepository,
        IAnswerOptionRepository answerOptionRepository,
        UserManager<AuthUser> userManager,
        ILogger<StoryPlayingController> logger)
    {
        _playingSessionRepository = playingSessionRepository;
        _storyRepository = storyRepository;
        _sceneRepository = sceneRepository;
        _answerOptionRepository = answerOptionRepository;
        _userManager = userManager;
        _logger = logger;
    }

    // ============================================================
    // 1. Story selection / join private
    // ============================================================

    [HttpGet("stories")]
    public async Task<IActionResult> GetStories([FromQuery] string? search = null)
    {
        try
        {
            var publicStories = await _storyRepository.GetAllPublicStories();
            var privateStories = await _storyRepository.GetAllPrivateStories();

            if (!string.IsNullOrWhiteSpace(search))
                publicStories = publicStories
                    .Where(s => s.Title.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            var dto = new StorySelectionDto
            {
                PublicStories = publicStories,
                PrivateStories = privateStories
            };

            return Ok(dto);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while fetching stories");
            return StatusCode(500, new ErrorDto { ErrorTitle = "Error loading stories" });
        }
    }

    [HttpPost("join")]
    public async Task<IActionResult> JoinPrivateStory([FromBody] StartPrivateStoryDto model)
    {
        if (string.IsNullOrWhiteSpace(model.Code))
            return BadRequest(new ErrorDto { ErrorTitle = "Code required" });

        try
        {
            var story = await _storyRepository.GetPrivateStoryByCode(model.Code.Trim().ToUpper());
            if (story == null)
                return NotFound(new ErrorDto { ErrorTitle = "No story found with that code" });

            return Ok(new { StoryId = story.StoryId, story.Title, story.Description });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to join private story");
            return StatusCode(500, new ErrorDto { ErrorTitle = "Failed to join story" });
        }
    }

    // ============================================================
    // 2. Start / confirm playing session
    // ============================================================

    [HttpPost("start/{storyId}")]
    public async Task<IActionResult> StartStory(int storyId, [FromBody] StartPrivateStoryDto? model = null)
    {
        try
        {
            var story = await _storyRepository.GetStoryById(storyId);
            if (story == null)
                return NotFound(new ErrorDto { ErrorTitle = "Story not found" });

            if (story.Accessibility == Accessibility.Private)
            {
                if (model == null || model.Code != story.Code)
                    return BadRequest(new ErrorDto { ErrorTitle = "Invalid code" });
            }

            var session = await BeginPlayingSessionAsync(story);

            return Ok(new
            {
                SessionId = session.PlayingSessionId,
                SceneId = session.CurrentSceneId,
                SceneType = session.CurrentSceneType
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error starting story {StoryId}", storyId);
            return StatusCode(500, new ErrorDto { ErrorTitle = "Failed to start story" });
        }
    }

    private async Task<PlayingSession> BeginPlayingSessionAsync(Story story)
    {
        var user = await _userManager.GetUserAsync(User)
            ?? throw new Exception("User not logged in.");

        await _storyRepository.IncrementPlayed(story.StoryId);

        var introScene = await _sceneRepository.GetIntroSceneByStoryId(story.StoryId)
            ?? throw new Exception("IntroScene missing");

        var amountOfQuestions = await _storyRepository.GetAmountOfQuestionsForStory(story.StoryId) ?? 0;
        var maxScore = Math.Max(amountOfQuestions * 10, 0);

        var session = new PlayingSession
        {
            StartTime = DateTime.UtcNow,
            Score = 0,
            MaxScore = maxScore,
            CurrentLevel = 3,
            CurrentSceneId = introScene.IntroSceneId,
            CurrentSceneType = SceneType.Intro,
            StoryId = story.StoryId,
            UserId = user.Id
        };

        await _playingSessionRepository.AddPlayingSession(session);
        return session;
    }

    // ============================================================
    // 3. Play Scene
    // ============================================================

    [HttpGet("scene")]
    public async Task<IActionResult> GetScene(int sceneId, SceneType sceneType, int sessionId)
    {
        try
        {
            object? scene = sceneType switch
            {
                SceneType.Intro => await _sceneRepository.GetIntroSceneById(sceneId),
                SceneType.Question => await _sceneRepository.GetQuestionSceneWithAnswerOptionsById(sceneId),
                SceneType.Ending => await _sceneRepository.GetEndingSceneById(sceneId),
                _ => null
            };

            if (scene == null)
                return NotFound(new ErrorDto { ErrorTitle = "Scene not found" });

            var session = await _playingSessionRepository.GetPlayingSessionById(sessionId);
            if (session == null)
                return NotFound(new ErrorDto { ErrorTitle = "Session not found" });

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
                    ? FilterAnswerOptionsByLevelAsync(((QuestionScene)scene).AnswerOptions, session.CurrentLevel)
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

            return Ok(dto);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error loading scene");
            return StatusCode(500, new ErrorDto { ErrorTitle = "Failed to load scene" });
        }
    }

    private static IEnumerable<AnswerOption> FilterAnswerOptionsByLevelAsync(IEnumerable<AnswerOption> allOptions, int level)
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

    private async Task<QuestionScene?> GetFirstQuestionSceneAsync(int storyId)
    {
        // 1. Get the first QuesionScene
        var firstQuestionScene = await _sceneRepository.GetFirstQuestionSceneByStoryId(storyId);

        // 2. Add the QuestionSceneId to the DTO
        if (firstQuestionScene != null)
        {
            return firstQuestionScene;
        }
        else
        {
            // If there are no questions, the game may go straight to the Ending, but we assume there are.
            // If this happens, you should log an error message
            _logger.LogWarning("Story {StoryId} has IntroScene but no QuestionScenes.", storyId);
            return null;
        }
    }

    // ============================================================
    // 4. Answer feedback and transitions
    // ============================================================

    [HttpPost("answer")]
    public async Task<IActionResult> SubmitAnswer([FromBody] AnswerFeedbackDto model)
    {
        try
        {
            var selectedAnswer = await _answerOptionRepository.GetAnswerOptionById(model.SelectedAnswerId);
            if (selectedAnswer == null)
                return NotFound(new ErrorDto { ErrorTitle = "Answer not found" });

            var session = await _playingSessionRepository.GetPlayingSessionById(model.SessionId);
            if (session == null)
                return NotFound(new ErrorDto { ErrorTitle = "Session not found" });

            bool isCorrect = selectedAnswer.IsCorrect;
            int points = isCorrect ? GetPointsForCorrectAnswerAsync(session.CurrentLevel) : 0;
            int newScore = session.Score + points;
            int newLevel = isCorrect
                ? Math.Min(session.CurrentLevel + 1, 3)
                : Math.Max(session.CurrentLevel - 1, 1);

            if (!isCorrect && session.CurrentLevel == 1)
            {
                await _playingSessionRepository.FinishSession(model.SessionId, newScore, newLevel);
                await _storyRepository.IncrementFailed(session.StoryId);
                return Ok(new { message = "Game over", score = newScore });
            }

            var feedback = new AnswerFeedbackDto
            {
                SceneText = selectedAnswer.FeedbackText,
                NewScore = newScore,
                NewLevel = newLevel
            };

            return Ok(feedback);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error submitting answer");
            return StatusCode(500, new ErrorDto { ErrorTitle = "Failed to process answer" });
        }
    }

    private static int GetPointsForCorrectAnswerAsync(int level) =>
        level switch { 3 => 5, 2 => 3, 1 => 1, _ => 0 };



    // ============================================================
    // 5. Get EndingScene
    // ============================================================

    [HttpGet("calculate-ending/{sessionId}")]
    public async Task<IActionResult> CalculateEnding(int sessionId)
    {
        var session = await _playingSessionRepository.GetPlayingSessionById(sessionId);
        if (session == null)
            return NotFound(new ErrorDto { ErrorTitle = "Session not found" });

        // Hent MaxScore (du må sørge for at MaxScore er tilgjengelig, enten i session eller Story)
        int maxScore = session.MaxScore; // Antar den er lagret i PlayingSession

        // Bruk din eksisterende logikk til å velge EndingScene (implementert privat i Controller/Service)
        var finalEndingScene = await GetEndingSceneAsync(session.StoryId, session.Score, maxScore);

        // VIKTIG: Lagre den valgte ID-en i databasen for å låse resultatet.
        //await _playingSessionRepository.SetFinalEndingScene(sessionId, finalEndingScene.EndingSceneId);

        // Returner kun ID-en til Frontend
        return Ok(new CalculateEndingResponseDto { EndingSceneId = finalEndingScene.EndingSceneId });
    }

    // Private method to decide which Ending Scene the user sees at the end of the story
    private async Task<EndingScene?> GetEndingSceneAsync(int storyId, int score, int maxScore)
    {
        if (maxScore <= 0)
            throw new ArgumentException($"Max score must be greater than zero, but it is {maxScore}.");

        double percentage = (double)score / maxScore * 100;

        EndingScene? endingScene = percentage switch
        {
            >= 80 => await _sceneRepository.GetGoodEndingSceneByStoryId(storyId),
            >= 40 => await _sceneRepository.GetNeutralEndingSceneByStoryId(storyId),
            _ => await _sceneRepository.GetBadEndingSceneByStoryId(storyId)
        };

        if (endingScene == null)
            throw new InvalidOperationException($"No ending scene found for story {storyId} (score {score}/{maxScore}).");

        return endingScene;
    }





    // ============================================================
    // 6. Finish story
    // ============================================================

    [HttpGet("finish/{sessionId:int}")]
    public async Task<IActionResult> FinishStory(int sessionId)
    {
        try
        {
            var session = await _playingSessionRepository.GetPlayingSessionById(sessionId);
            if (session == null)
                return NotFound(new ErrorDto { ErrorTitle = "Session not found" });

            var story = await _storyRepository.GetStoryById(session.StoryId);
            if (story == null)
                return NotFound(new ErrorDto { ErrorTitle = "Story not found" });

            var dto = new FinishStoryDto
            {
                StoryTitle = story.Title,
                FinalScore = session.Score,
                MaxScore = session.MaxScore
            };

            return Ok(dto);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error finishing story");
            return StatusCode(500, new ErrorDto { ErrorTitle = "Failed to finish story" });
        }
    }
}