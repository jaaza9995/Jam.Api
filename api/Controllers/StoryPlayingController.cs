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
        ILogger<StoryPlayingController> logger
    )
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
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new ErrorDto { ErrorTitle = "Not logged in" });
            var userId = user.Id;

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
            _logger.LogError(e, "[StoryPlayingController -> StartStory] Error starting story {StoryId}", storyId);
            return StatusCode(500, new ErrorDto { ErrorTitle = "Failed to start story" });
        }
    }




    // ----------------------------------------------------------------------------------------
    // 2. PLAY SCENE (fetches all types of scenes)
    // ----------------------------------------------------------------------------------------

    [HttpGet("scene")]
    public async Task<IActionResult> GetScene(int sceneId, SceneType sceneType, int sessionId)
    {
        try
        {
            var dto = await _storyPlayingService.BuildPlaySceneDtoAsync(sceneId, sceneType, sessionId);
            if (dto == null)
                return NotFound(new ErrorDto { ErrorTitle = "Scene not found" });

            if (sceneType == SceneType.Question)
            {
                var success = await _storyPlayingService.TransitionFromIntroToFirstQuestionAsync(sessionId, sceneId);
                if (!success)
                    _logger.LogWarning("[StoryPlayingController -> GetScene] Failed Intro->Q transition for SessionId: {sessionId}", sessionId);
            }

            return Ok(dto);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[StoryPlayingController -> GetScene] Error loading scene");
            return StatusCode(500, new ErrorDto { ErrorTitle = "Failed to load scene" });
        }
        /*
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
                        _logger.LogWarning("[StoryPlayingController -> GetScene] Failed to update session progress (Intro -> Q) for SessionId: {sessionId}", sessionId);
                    }
                }
            }

            return Ok(dto);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[StoryPlayingController -> GetScene] Error loading scene");
            return StatusCode(500, new ErrorDto { ErrorTitle = "Failed to load scene" });
        }
        */
    }




    // ----------------------------------------------------------------------------------------
    // 3. USER ANSWERS QUESTION (story progression logic)
    // ----------------------------------------------------------------------------------------

    [HttpPost("answer")]
    public async Task<IActionResult> SubmitAnswer([FromBody] AnswerFeedbackDto model)
    {
        try
        {
            var feedback = await _storyPlayingService.ProcessAnswerAsync(model.SelectedAnswerId, model.SessionId);
            if (feedback.IsGameOver)
            {
                return Ok(new { message = "Game over", score = feedback.NewScore });
            }
            
            return Ok(feedback);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new ErrorDto { ErrorTitle = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "[StoryPlayingController -> SubmitAnswer] Business logic error");
            return StatusCode(500, new ErrorDto { ErrorTitle = ex.Message });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[StoryPlayingController -> SubmitAnswer] Error submitting answer");
            return StatusCode(500, new ErrorDto { ErrorTitle = "Failed to process answer" });
        }

        /*
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

            // Handling Game Over (Level 1 Fail)
            if (!isCorrect && session.CurrentLevel == 1)
            {
                // No need to update nextSceneId/Type in DB, only FinishSession
                await _playingSessionRepository.FinishSession(model.SessionId, newScore, newLevel);
                await _storyRepository.IncrementFailed(session.StoryId);
                return Ok(new { message = "Game over", score = newScore });
            }

            if (!nextSceneId.HasValue) // We are at the last QuestionScene -> Going to EndingScene
            {
                // Calculate which Ending Scene ID the user gets based on user's score
                var finalEndingScene = await _storyPlayingService.GetEndingSceneAsync(session.StoryId, newScore, session.MaxScore);

                if (finalEndingScene != null)
                {
                    nextSceneId = finalEndingScene.EndingSceneId;
                    nextSceneType = SceneType.Ending;
                    await _storyRepository.IncrementFinished(session.StoryId);
                }
                else
                {
                    // Error: Cannot find the end scene, log and return error
                    _logger.LogError("[StoryPlayingController -> SubmitAnswer] Could not find an appropriate ending scene for story {StoryId} at score {Score}.", session.StoryId, newScore);
                    return StatusCode(500, new ErrorDto { ErrorTitle = "Failed to fin the right ending scene." });
                }
            }

            // Update PlayingSession in the database (moving progress)
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

            // Return Feedback to Frontend (Includes next scene to trigger fetchScene)
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
            _logger.LogError(e, "[StoryPlayingController] Error submitting answer");
            return StatusCode(500, new ErrorDto { ErrorTitle = "Failed to process answer" });
        }
        */
    }
}