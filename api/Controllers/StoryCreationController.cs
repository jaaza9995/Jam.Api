using Jam.Api.DAL.AnswerOptionDAL;
using Jam.Api.DAL.SceneDAL;
using Jam.Api.DAL.StoryDAL;
using Jam.DTOs.Story;
using Jam.DTOs.IntroScenes;
using Jam.DTOs.QuestionScenes;
using Jam.DTOs.UpdateEndingScenes;
using Jam.DTOs;
using Jam.Extensions;
using Jam.Models;
using Jam.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Jam.Api.Services;
using Jam.DTOs.StoryCreationSession;



namespace Jam.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoryCreationController : ControllerBase
{
    private readonly IStoryRepository _storyRepo;
    private readonly StoryCodeService _codeService;
    private readonly UserManager<AuthUser> _userManager;
    private readonly ISceneRepository _sceneRepo;
    private readonly IAnswerOptionRepository _answerRepo;
    private readonly ILogger<StoryCreationController> _logger;

   public StoryCreationController(
    IStoryRepository storyRepo,
    ISceneRepository sceneRepo,
    IAnswerOptionRepository answerRepo,
    StoryCodeService codeService,
    UserManager<AuthUser> userManager,
    ILogger<StoryCreationController> logger)
{
    _storyRepo = storyRepo;
    _sceneRepo = sceneRepo;
    _answerRepo = answerRepo;
    _codeService = codeService;
    _userManager = userManager;
    _logger = logger;
}
    // =====================================================================
    // STEP 1 — STORY + INTRO
    // =====================================================================

    [HttpPost("intro")]
    public IActionResult SaveIntro([FromBody] CreateStoryDto storyDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var session = HttpContext.Session.GetObject<StoryCreationSession>("CreateStory")
                       ?? new StoryCreationSession();

        session.Title = storyDto.Title;
        session.Description = storyDto.Description;
        session.DifficultyLevel = storyDto.DifficultyLevel;
        session.Accessibility = storyDto.Accessibility;
        session.IntroText = storyDto.IntroText;

        HttpContext.Session.SetObject("CreateStory", session);

        return Ok(new { message = "Story intro saved." });
    }

    [HttpGet("intro")]
    public IActionResult GetIntro()
    {
        var session = HttpContext.Session.GetObject<StoryCreationSession>("CreateStory")
                     ?? new StoryCreationSession();

        return Ok(new
        {
            session.Title,
            session.Description,
            session.DifficultyLevel,
            session.Accessibility,
            session.IntroText
        });
    }

    // =====================================================================
    // STEP 2 — INTRO TEXT
    // =====================================================================

    [HttpPost("intro-text")]
    public IActionResult SaveIntroText([FromBody] IntroSceneDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var session = HttpContext.Session.GetObject<StoryCreationSession>("CreateStory")
                     ?? new StoryCreationSession();

        session.IntroText = dto.IntroText;

        HttpContext.Session.SetObject("CreateStory", session);

        return Ok(new { message = "Intro text saved." });
    }

    [HttpGet("intro-text")]
    public IActionResult GetIntroText()
    {
        var session = HttpContext.Session.GetObject<StoryCreationSession>("CreateStory")
                     ?? new StoryCreationSession();

        return Ok(new { session.IntroText });
    }

    // =====================================================================
    // STEP 3 — QUESTION SCENES
    // =====================================================================

    [HttpGet("questions")]
    public IActionResult GetQuestions()
    {
        var session = HttpContext.Session.GetObject<StoryCreationSession>("CreateStory")
                     ?? new StoryCreationSession();

        return Ok(session.QuestionScenes);
    }

   [HttpPost("questions")]
public IActionResult SaveQuestions([FromBody] QuestionScenesPayload payload)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    var session = HttpContext.Session.GetObject<StoryCreationSession>("CreateStory")
                ?? new StoryCreationSession();

    session.QuestionScenes = payload.QuestionScenes.Select(q => new QuestionSceneDto
    {
        QuestionSceneId = q.QuestionSceneId,
        StoryText = q.StoryText,
        QuestionText = q.QuestionText,
        CorrectAnswerIndex = q.CorrectAnswerIndex,
        Answers = q.Answers
    }).ToList();

    HttpContext.Session.SetObject("CreateStory", session);

    // 🔥 Samme logikk som i HomeController, men uten per-question felt
    var questionCount = session.QuestionScenes.Count;

    return Ok(new 
    { 
        message = "Questions saved.",
        questionCount
    });
}



    // =====================================================================
    // STEP 4 — ENDING SCENES
    // =====================================================================

    [HttpGet("endings")]
    public IActionResult GetEndings()
    {
        var session = HttpContext.Session.GetObject<StoryCreationSession>("CreateStory")
                     ?? new StoryCreationSession();

        return Ok(new UpdateEndingSceneDto
        {
            StoryId = 0,
            GoodEnding = session.GoodEnding,
            NeutralEnding = session.NeutralEnding,
            BadEnding = session.BadEnding
        });
    }


    [HttpPost("endings")]
    public IActionResult SaveEndings([FromBody] UpdateEndingSceneDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var session = HttpContext.Session.GetObject<StoryCreationSession>("CreateStory");
        if (session == null)
            return BadRequest(new ErrorDto { ErrorTitle = "Session expired" });

        session.GoodEnding = dto.GoodEnding;
        session.NeutralEnding = dto.NeutralEnding;
        session.BadEnding = dto.BadEnding;

        HttpContext.Session.SetObject("CreateStory", session);

        return Ok(new { message = "Endings saved." });
    }


    // =====================================================================
    // FINAL STEP — CREATE STORY IN DATABASE
    // =====================================================================

    [HttpPost("create")]
    public async Task<IActionResult> CreateStory()
    {
        var session = HttpContext.Session.GetObject<StoryCreationSession>("CreateStory");
        if (session == null)
            return BadRequest(new ErrorDto { ErrorTitle = "Session expired" });

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized(new ErrorDto { ErrorTitle = "Not logged in" });

        var story = new Story
        {
            Title = session.Title,
            Description = session.Description,
            DifficultyLevel = session.DifficultyLevel,
            Accessibility = session.Accessibility,
            UserId = user.Id,
            IntroScene = new IntroScene { IntroText = session.IntroText },
            QuestionScenes = session.QuestionScenes.Select(q => new QuestionScene
            {
                SceneText = q.StoryText,
                Question = q.QuestionText,
                AnswerOptions = q.Answers.Select((a, idx) => new AnswerOption
                {
                    Answer = a.AnswerText,
                    FeedbackText = a.ContextText,
                    IsCorrect = idx == q.CorrectAnswerIndex
                }).ToList()
            }).ToList(),
            EndingScenes = new List<EndingScene>
            {
                new EndingScene { EndingType = EndingType.Good, EndingText = session.GoodEnding },
                new EndingScene { EndingType = EndingType.Neutral, EndingText = session.NeutralEnding },
                new EndingScene { EndingType = EndingType.Bad, EndingText = session.BadEnding }
            }
        };

        if (story.Accessibility == Accessibility.Private)
        {
            story.Code = await _codeService.GenerateUniqueStoryCodeAsync();
        }

        var saved = await _storyRepo.AddFullStory(story);
        if (!saved)
            return StatusCode(500, new ErrorDto { ErrorTitle = "Could not save story" });

        HttpContext.Session.Remove("CreateStory");

        return Ok(new { message = "Story created!", storyId = story.StoryId });
    }

// =====================================================================
// INTERNAL: Session DTO used only inside the controller
// =====================================================================
}
