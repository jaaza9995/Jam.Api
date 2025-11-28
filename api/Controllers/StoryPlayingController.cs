using Jam.Api.DAL.AnswerOptionDAL;
using Jam.Api.DAL.PlayingSessionDAL;
using Jam.Api.DAL.SceneDAL;
using Jam.Api.DAL.StoryDAL;
using Jam.Api.DTOs.StoryPlaying;
using Jam.Api.DTOs.Shared;
using Jam.Api.Models;
using Jam.Api.Models.Enums;
using Jam.Api.Services;
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
    private readonly IStoryPlayingService _storyPlayingService;
    private readonly ILogger<StoryPlayingController> _logger;

    public StoryPlayingController(
        IPlayingSessionRepository playingSessionRepository,
        IStoryRepository storyRepository,
        ISceneRepository sceneRepository,
        IAnswerOptionRepository answerOptionRepository,
        UserManager<AuthUser> userManager,
        IStoryPlayingService storyPlayingService,
        ILogger<StoryPlayingController> logger)
    {
        _playingSessionRepository = playingSessionRepository;
        _storyRepository = storyRepository;
        _sceneRepository = sceneRepository;
        _answerOptionRepository = answerOptionRepository;
        _userManager = userManager;
        _storyPlayingService = storyPlayingService;
        _logger = logger;
    }


    // ----------------------------------------------------------------------------------------
    // 1. START STORY (begin playing session)
    // ----------------------------------------------------------------------------------------
    [HttpPost("start/{storyId}")]
    public async Task<IActionResult> StartStory(int storyId)
    {
        try
        {
            // 1. Get UserId
            var userId = GetCurrentUserId();

            // 2. Get Story and validate access
            var story = await _storyRepository.GetStoryById(storyId);
            if (story == null)
                return NotFound(new ErrorDto { ErrorTitle = "Story not found" });

            // 3. Begin PlayingSession with private method
            var session = await _storyPlayingService.BeginPlayingSessionAsync(story, userId!);
            if (session == null)
            {
                return StatusCode(500, new ErrorDto { ErrorTitle = "Failed to start playing session" });
            }

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

    private string? GetCurrentUserId()
    {
        // Henter UserId (Sub) fra JWT-tokenets Claims
        // "NameIdentifier" eller "Sub" er standard claims for UserId i ASP.NET Core
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }



    // ----------------------------------------------------------------------------------------
    // 2. PLAY SCENE (fetches all types of scenes)
    // ----------------------------------------------------------------------------------------
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
                    ? _storyPlayingService.FilterAnswerOptionsByLevelAsync(((QuestionScene)scene).AnswerOptions, session.CurrentLevel)
                    : null,
                NextSceneAfterIntroId = null,
                CurrentScore = session.Score,
                MaxScore = session.MaxScore,
                CurrentLevel = session.CurrentLevel
            };

            if (sceneType == SceneType.Intro)
            {
                var firstQuestionScene = await _storyPlayingService.GetFirstQuestionSceneAsync(session.StoryId);
                dto.NextSceneAfterIntroId = firstQuestionScene?.QuestionSceneId;
            }

            // --- TRANSITION LOGIC: Intro -> First QuestionScene ---
            // Criterion 1: Are we entering a QuestionScene?
            if (sceneType == SceneType.Question)
            {
                // Criterion 2: Check if PlayingSession in DB still points to IntroScene.
                // (This indicates that this is the first time we are retrieving QuestionScene data)
                if (session.CurrentSceneType == SceneType.Intro)
                {
                    // Update PlayingSession in DB
                    bool success = await _playingSessionRepository.TransitionFromIntroToFirstQuestion(
                        sessionId,
                        sceneId,
                        SceneType.Question
                    );

                    if (!success)
                    {
                        _logger.LogWarning("Failed to update session progress (Intro -> Q) for SessionId: {sessionId}", sessionId);
                    }
                }
            }

            return Ok(dto);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error loading scene");
            return StatusCode(500, new ErrorDto { ErrorTitle = "Failed to load scene" });
        }
    }


    // ----------------------------------------------------------------------------------------
    // 3. USER ANSWERS QUESTION (story progression logic)
    // ----------------------------------------------------------------------------------------
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
            int points = isCorrect ? _storyPlayingService.GetPointsForCorrectAnswerAsync(session.CurrentLevel) : 0;
            int newScore = session.Score + points;
            int newLevel = isCorrect
                ? Math.Min(session.CurrentLevel + 1, 3)
                : Math.Max(session.CurrentLevel - 1, 1);

            // Get the next QuestionScene
            int currentQuestionSceneId = session.CurrentSceneId.GetValueOrDefault();
            var nextScene = await _sceneRepository.GetNextQuestionSceneById(currentQuestionSceneId);

            int? nextSceneId = nextScene?.QuestionSceneId; // can be null if it is the last QuestionScene
            SceneType nextSceneType = nextScene != null ? SceneType.Question : SceneType.Ending;

            // 4. Håndtering av Game Over (Level 1 Fail)
            if (!isCorrect && session.CurrentLevel == 1)
            {
                // Vi trenger ikke å oppdatere nextSceneId/Type i DB, kun FinishSession
                await _playingSessionRepository.FinishSession(model.SessionId, newScore, newLevel);
                await _storyRepository.IncrementFailed(session.StoryId);
                return Ok(new { message = "Game over", score = newScore });
            }

            if (!nextSceneId.HasValue) // Vi er på siste QuestionScene -> Skal til EndingScene
            {
                // 1. Beregn hvilken Ending Scene ID brukeren får basert på poeng
                var finalEndingScene = await _storyPlayingService.GetEndingSceneAsync(session.StoryId, newScore, session.MaxScore);

                if (finalEndingScene != null)
                {
                    nextSceneId = finalEndingScene.EndingSceneId; // Sett den FAKTISKE Ending Scene ID-en
                    nextSceneType = SceneType.Ending; // Sikkerhetssteg, men burde være satt
                    await _storyRepository.IncrementFinished(session.StoryId);
                }
                else
                {
                    // Feil: Finner ikke sluttscenen, logg og returner feil.
                    _logger.LogError("Could not find an appropriate ending scene for story {StoryId} at score {Score}.", session.StoryId, newScore);
                    return StatusCode(500, new ErrorDto { ErrorTitle = "Failed to fin the right ending scene." });
                }
            }

            // 5. Oppdater PlayingSession i databasen (flytter fremgang)
            if (nextSceneId.HasValue)
            {
                await _playingSessionRepository.AnswerQuestion(
                    model.SessionId,
                    nextSceneId,        // has either QuestionSceneId or EndingSceneId
                    nextSceneType,      // has either Question or Ending as SceneType
                    newScore,
                    newLevel
                );
            }

            // 6. Returner Feedback til Frontend (Inkluderer neste scene for å trigge fetchScene)
            var feedback = new AnswerFeedbackDto
            {
                SceneText = selectedAnswer.FeedbackText,
                NewScore = newScore,
                NewLevel = newLevel,
                NextSceneId = nextSceneId,
                NextSceneType = nextSceneType
            };

            return Ok(feedback);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error submitting answer");
            return StatusCode(500, new ErrorDto { ErrorTitle = "Failed to process answer" });
        }
    }
}