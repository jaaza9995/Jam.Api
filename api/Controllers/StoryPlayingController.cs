using Jam.DAL.AnswerOptionDAL;
using Jam.DAL.PlayingSessionDAL;
using Jam.DAL.SceneDAL;
using Jam.DAL.StoryDAL;
using Jam.DTOs;
using Jam.DTOs.StoryPlaying;
using Jam.Models;
using Jam.Models.Enums;
using Jam.DTOs.JoinPrivateStoryRequestDto;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

    private string? GetCurrentUserId() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    // =====================================================================
    // 1. STORY LIST / PRIVATE LOGIN
    // =====================================================================

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
                PublicStories = publicStories.Select(ToCardDto).ToList(),
                PrivateStories = privateStories.Select(ToCardDto).ToList()
            };

            return Ok(dto);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error loading stories");
            return StatusCode(500, new ErrorDto { ErrorTitle = "Error loading stories" });
        }
    }

    private StoryCardDto ToCardDto(Story s) =>
        new StoryCardDto
        {
            StoryId = s.StoryId,
            Title = s.Title,
            Description = s.Description,
            QuestionCount = s.QuestionCount,
            Accessibility = s.Accessibility.ToString(),
            Code = s.Code ?? ""
        };

    [HttpPost("join")]
    public async Task<IActionResult> JoinPrivateStory([FromBody] JoinPrivateStoryRequestDto model)
    {
         if (!ModelState.IsValid)
            return BadRequest(ModelState);
            
        if (string.IsNullOrWhiteSpace(model.Code))
            return BadRequest(new ErrorDto { ErrorTitle = "Code required" });

        try
        {
            var story = await _storyRepository.GetPrivateStoryByCode(
                model.Code.Trim().ToUpper());

            if (story == null)
                return NotFound(new ErrorDto { ErrorTitle = "Invalid code" });

            return Ok(new StoryCardDto
            {
                StoryId = story.StoryId,
                Title = story.Title,
                Description = story.Description,
                QuestionCount = story.QuestionCount,
                Accessibility = story.Accessibility.ToString(),
                Code = story.Code ?? ""
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error joining private story");
            return StatusCode(500, new ErrorDto { ErrorTitle = "Failed to join story" });
        }
    }

    // =====================================================================
    // 2. START PLAYING
    // =====================================================================

    [HttpPost("start/{storyId}")]
    public async Task<IActionResult> StartStory(
        int storyId,
        [FromBody] JoinPrivateStoryRequestDto? codeModel = null)
    {
         if (!ModelState.IsValid)
            return BadRequest(ModelState);
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ErrorDto { ErrorTitle = "User not authenticated" });

            var story = await _storyRepository.GetStoryById(storyId);
            if (story == null)
                return NotFound(new ErrorDto { ErrorTitle = "Story not found" });

            if (story.Accessibility == Accessibility.Private)
            {
                if (codeModel == null || codeModel.Code != story.Code)
                    return BadRequest(new ErrorDto { ErrorTitle = "Invalid code" });
            }

            var session = await BeginSessionAsync(story, userId);

            return Ok(new StartStoryResponseDto
            {
                SessionId = session.PlayingSessionId,
                SceneId = session.CurrentSceneId,
                SceneType = session.CurrentSceneType
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error starting story {storyId}", storyId);
            return StatusCode(500, new ErrorDto { ErrorTitle = "Failed to start story" });
        }
    }

    private async Task<PlayingSession> BeginSessionAsync(Story story, string userId)
    {
        await _storyRepository.IncrementPlayed(story.StoryId);

        var intro = await _sceneRepository.GetIntroSceneByStoryId(story.StoryId)
            ?? throw new Exception("IntroScene missing");

        var questionCount = await _storyRepository.GetAmountOfQuestionsForStory(story.StoryId) ?? 0;

        var maxScore = questionCount * 10;

        var session = new PlayingSession
        {
            StoryId = story.StoryId,
            UserId = userId,
            StartTime = DateTime.UtcNow,
            Score = 0,
            MaxScore = maxScore,
            CurrentLevel = 3,
            CurrentSceneId = intro.IntroSceneId,
            CurrentSceneType = SceneType.Intro
        };

        await _playingSessionRepository.AddPlayingSession(session);

        return session;
    }

    // =====================================================================
    // 3. GET SCENE
    // =====================================================================

    [HttpGet("scene")]
    public async Task<IActionResult> GetScene(int sceneId, SceneType sceneType, int sessionId)
    {
        try
        {
            var session = await _playingSessionRepository.GetPlayingSessionById(sessionId);
            if (session == null)
                return NotFound(new ErrorDto { ErrorTitle = "Session not found" });

            object? scene = sceneType switch
            {
                SceneType.Intro => await _sceneRepository.GetIntroSceneById(sceneId),
                SceneType.Question => await _sceneRepository.GetQuestionSceneWithAnswerOptionsById(sceneId),
                SceneType.Ending => await _sceneRepository.GetEndingSceneById(sceneId),
                _ => null
            };

            if (scene == null)
                return NotFound(new ErrorDto { ErrorTitle = "Scene not found" });

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
                    _ => ""
                },
                Question = sceneType == SceneType.Question
                    ? ((QuestionScene)scene).Question
                    : null,
                CurrentScore = session.Score,
                MaxScore = session.MaxScore,
                CurrentLevel = session.CurrentLevel
            };

            if (sceneType == SceneType.Intro)
            {
                var firstQ = await _sceneRepository.GetFirstQuestionSceneByStoryId(session.StoryId);
                dto.NextSceneAfterIntroId = firstQ?.QuestionSceneId;
            }

            if (sceneType == SceneType.Question)
            {
                var qScene = (QuestionScene)scene;
                var filtered = FilterAnswerOptions(qScene.AnswerOptions, session.CurrentLevel);

                dto.AnswerOptions = filtered.Select(a => new PlayAnswerOptionDto
                {
                    AnswerOptionId = a.AnswerOptionId,
                    AnswerText = a.Answer
                }).ToList();
            }

            return Ok(dto);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error loading scene");
            return StatusCode(500, new ErrorDto { ErrorTitle = "Failed to load scene" });
        }
    }

    private static List<AnswerOption> FilterAnswerOptions(
        List<AnswerOption> allOptions, int level)
    {
        var correct = allOptions.First(a => a.IsCorrect);
        var wrong = allOptions.Where(a => !a.IsCorrect).ToList();

        var random = new Random();

        int count = level switch
        {
            3 => 4,
            2 => 3,
            1 => 2,
            _ => 4
        };

        var selected = new List<AnswerOption> { correct };
        selected.AddRange(
            wrong.OrderBy(_ => random.Next()).Take(count - 1)
        );

        return selected.OrderBy(_ => random.Next()).ToList();
    }

    // =====================================================================
    // 4. SUBMIT ANSWER
    // =====================================================================

    [HttpPost("answer")]
    public async Task<IActionResult> SubmitAnswer([FromBody] SubmitAnswerRequestDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        try
        {
            var session = await _playingSessionRepository.GetPlayingSessionById(model.SessionId);
            if (session == null)
                return NotFound(new ErrorDto { ErrorTitle = "Session not found" });

            var answer = await _answerOptionRepository.GetAnswerOptionById(model.SelectedAnswerId);
            if (answer == null)
                return NotFound(new ErrorDto { ErrorTitle = "Answer not found" });

            bool correct = answer.IsCorrect;

            int points = correct
                ? (session.CurrentLevel == 3 ? 5 :
                   session.CurrentLevel == 2 ? 3 : 1)
                : 0;

            int newScore = session.Score + points;

            int newLevel = correct
                ? Math.Min(session.CurrentLevel + 1, 3)
                : Math.Max(session.CurrentLevel - 1, 1);

            if (!correct && session.CurrentLevel == 1)
            {
                await _playingSessionRepository.FinishSession(model.SessionId, newScore, newLevel);
                await _storyRepository.IncrementFailed(session.StoryId);

                return Ok(new
                {
                    message = "Game over",
                    score = newScore
                });
            }

            var dto = new AnswerFeedbackDto
            {
                SceneText = answer.FeedbackText,
                NewScore = newScore,
                NewLevel = newLevel
            };

            return Ok(dto);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error submitting answer");
            return StatusCode(500, new ErrorDto { ErrorTitle = "Failed to process answer" });
        }
    }

    // =====================================================================
    // 5. CALCULATE ENDING
    // =====================================================================

    [HttpGet("calculate-ending/{sessionId}")]
    public async Task<IActionResult> CalculateEnding(int sessionId)
    {
        var session = await _playingSessionRepository.GetPlayingSessionById(sessionId);
        if (session == null)
            return NotFound(new ErrorDto { ErrorTitle = "Session not found" });

        int max = session.MaxScore;
        int score = session.Score;

        var ending = await GetEndingSceneAsync(session.StoryId, score, max);

        return Ok(new CalculateEndingResponseDto
        {
            EndingSceneId = ending.EndingSceneId
        });
    }

    private async Task<EndingScene> GetEndingSceneAsync(int storyId, int score, int max)
{
    double pct = (double)score / max * 100;

    EndingScene? scene = pct switch
    {
        >= 80 => await _sceneRepository.GetGoodEndingSceneByStoryId(storyId),
        >= 40 => await _sceneRepository.GetNeutralEndingSceneByStoryId(storyId),
        _ => await _sceneRepository.GetBadEndingSceneByStoryId(storyId)
    };

    if (scene == null)
        throw new InvalidOperationException($"No ending scene found for story {storyId}");

    return scene;
}


    // =====================================================================
    // 6. FINISH STORY
    // =====================================================================

    [HttpGet("finish/{sessionId}")]
    public async Task<IActionResult> FinishStory(int sessionId)
    {
        var session = await _playingSessionRepository.GetPlayingSessionById(sessionId);
        if (session == null)
            return NotFound(new ErrorDto { ErrorTitle = "Session not found" });

        var story = await _storyRepository.GetStoryById(session.StoryId);
        if (story == null)
            return NotFound(new ErrorDto { ErrorTitle = "Story not found" });

        return Ok(new FinishStoryDto
        {
            StoryTitle = story.Title,
            FinalScore = session.Score,
            MaxScore = session.MaxScore
        });
    }
}
