using Jam.Api.DAL.StoryDAL;
using Jam.Api.DTOs.StoryPlaying;
using Jam.Api.DTOs.Shared;
using Jam.Api.Models;
using Jam.Api.Models.Enums;
using Jam.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Jam.Api.DAL.PlayingSessionDAL;


namespace Jam.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoryPlayingController : ControllerBase
{
    private readonly IStoryRepository _storyRepository;
    private readonly IPlayingSessionRepository _playingSessionRepository;
    private readonly UserManager<AuthUser> _userManager;
    private readonly IStoryPlayingService _storyPlayingService;
    private readonly ILogger<StoryPlayingController> _logger;

    public StoryPlayingController(
        IStoryRepository storyRepository,
        IPlayingSessionRepository playingSessionRepository,
        UserManager<AuthUser> userManager,
        IStoryPlayingService storyPlayingService,
        ILogger<StoryPlayingController> logger
    )
    {
        _storyRepository = storyRepository;
        _playingSessionRepository = playingSessionRepository;
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
                var session = await _playingSessionRepository.GetPlayingSessionById(sessionId);
                if (session?.CurrentSceneType == SceneType.Intro) // Only transition from Intro -> Question if the session is currently at Intro
                {
                    var success = await _storyPlayingService.TransitionFromIntroToFirstQuestionAsync(sessionId, sceneId);
                    if (!success)
                        _logger.LogWarning("[StoryPlayingController -> GetScene] Failed Intro -> first QuestionScene transition for SessionId: {sessionId}", sessionId);
                }
            }

            return Ok(dto);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[StoryPlayingController -> GetScene] Error loading scene");
            return StatusCode(500, new ErrorDto { ErrorTitle = "Failed to load scene" });
        }
    }




    // ----------------------------------------------------------------------------------------
    // 3. USER ANSWERS QUESTION 
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
    }
}