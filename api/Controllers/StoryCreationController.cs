using Jam.Api.DAL.AnswerOptionDAL;
using Jam.Api.DAL.SceneDAL;
using Jam.Api.DAL.StoryDAL;
using Jam.DTOs;
using Jam.DTOs.StoryCreation;
using Jam.Extensions;
using Jam.Models;
using Jam.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Jam.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoryCreationController : ControllerBase
{
    private readonly IStoryRepository _storyRepository;
    private readonly ISceneRepository _sceneRepository;
    private readonly IAnswerOptionRepository _answerOptionRepository;
    private readonly UserManager<AuthUser> _userManager;
    private readonly ILogger<StoryCreationController> _logger;

    public StoryCreationController(
        IStoryRepository storyRepository,
        ISceneRepository sceneRepository,
        IAnswerOptionRepository answerOptionRepository,
        UserManager<AuthUser> userManager,
        ILogger<StoryCreationController> logger)
    {
        _storyRepository = storyRepository;
        _sceneRepository = sceneRepository;
        _answerOptionRepository = answerOptionRepository;
        _userManager = userManager;
        _logger = logger;
    }

    // =====================================================================
    // STEP 1: Story + Intro
    // =====================================================================

    // Hent det som ligger i session (hvis bruker kommer tilbake til wizard)
    [HttpGet("intro")]
    public IActionResult GetStoryAndIntro()
    {
        try
        {
            var sessionDto = HttpContext.Session.GetObject<StoryCreationDto>("StoryCreation")
                  ?? new StoryCreationDto();

                // Hent bruker og lagre UserId i session
                var user = _userManager.GetUserAsync(User).Result;
                if (user == null)
                    return Unauthorized(new { message = "You must be logged in." });

                sessionDto.UserId = user.Id;
                if (sessionDto == null)

            {
                // Tomt utgangspunkt til frontend
                return Ok(new CreateStoryAndIntroDto());
            }

            var dto = new CreateStoryAndIntroDto
            {
                Title = sessionDto.Title,
                Description = sessionDto.Description,
                DifficultyLevel = sessionDto.DifficultyLevel,
                Accessibility = sessionDto.Accessibility,
                IntroText = sessionDto.IntroText
            };

            return Ok(dto);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[StoryCreationController -> GetStoryAndIntro] Failed to load intro from session");
            return StatusCode(500, new ErrorDto
            {
                ErrorTitle = "Failed to load story intro",
                ErrorMessage = "An unexpected error occurred while loading your story intro."
            });
        }
    }

   [HttpPost("intro")]
    public IActionResult SaveStoryAndIntro([FromBody] CreateStoryAndIntroDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var sessionDto = HttpContext.Session.GetObject<StoryCreationDto>("StoryCreation")
                            ?? new StoryCreationDto();

            // Hent innlogget bruker
            var user = _userManager.GetUserAsync(User).Result;
            if (user == null)
                return Unauthorized(new { message = "You must be logged in." });

            // Lagre userId i session
            sessionDto.UserId = user.Id;

            sessionDto.Title = model.Title;
            sessionDto.Description = model.Description;
            sessionDto.DifficultyLevel = model.DifficultyLevel;
            sessionDto.Accessibility = model.Accessibility;
            sessionDto.IntroText = model.IntroText;

            HttpContext.Session.SetObject("StoryCreation", sessionDto);

            return Ok(new { message = "Intro step saved successfully." });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[StoryCreation -> SaveStoryAndIntro] Failed");
            return StatusCode(500, new { message = "Error saving intro" });
        }
    }


    // Avbryt: tøm session
    [HttpPost("intro/cancel")]
    public IActionResult CancelStoryCreation()
    {
        HttpContext.Session.Remove("StoryCreation");
        return Ok(new { message = "Story creation cancelled and session cleared." });
    }

    // =====================================================================
    // STEP 2: Question Scenes
    // =====================================================================

    [HttpGet("questions")]
    public IActionResult GetQuestionScenes()
    {
        try
        {
            var sessionDto = HttpContext.Session.GetObject<StoryCreationDto>("StoryCreation");
            if (sessionDto == null)
            {
                return BadRequest(new ErrorDto
                {
                    ErrorTitle = "Session expired",
                    ErrorMessage = "Your session has expired. Please start creating your story again."
                });
            }

            var viewDto = new CreateMultipleQuestionScenesDto
            {
                QuestionScenes = sessionDto.QuestionScenes.Any()
                    ? sessionDto.QuestionScenes.Select(q => new QuestionSceneBaseDto
                    {
                        StoryText = q.StoryText,
                        QuestionText = q.QuestionText,
                        Answers = q.Answers.Select(a => new AnswerOptionInput
                        {
                            AnswerText = a.AnswerText,
                            ContextText = a.ContextText
                        }).ToList(),
                        CorrectAnswerIndex = q.CorrectAnswerIndex
                    }).ToList()
                    : new List<QuestionSceneBaseDto> { new QuestionSceneBaseDto() }
            };

            return Ok(viewDto);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[StoryCreationController -> GetQuestionScenes] Failed to load question scenes from session");
            return StatusCode(500, new ErrorDto
            {
                ErrorTitle = "Failed to load question scenes",
                ErrorMessage = "An unexpected error occurred while loading your question scenes."
            });
        }
    }

    [HttpPost("questions")]
    public IActionResult SaveQuestionScenes([FromBody] CreateMultipleQuestionScenesDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var sessionDto = HttpContext.Session.GetObject<StoryCreationDto>("StoryCreation")
                              ?? new StoryCreationDto();

            sessionDto.QuestionScenes = model.QuestionScenes.Select(q => new QuestionSceneDto
            {
                StoryText = q.StoryText,
                QuestionText = q.QuestionText,
                Answers = q.Answers.Select(a => new AnswerOptionDto
                {
                    AnswerText = a.AnswerText,
                    ContextText = a.ContextText
                }).ToList(),
                CorrectAnswerIndex = q.CorrectAnswerIndex
            }).ToList();

            HttpContext.Session.SetObject("StoryCreation", sessionDto); //skjekk den ut etterpå

            return Ok(new { message = "Question scenes saved successfully." });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[StoryCreationController -> SaveQuestionScenes] Failed to save question scenes to session");
            return StatusCode(500, new ErrorDto
            {
                ErrorTitle = "Failed to save question scenes",
                ErrorMessage = "An unexpected error occurred while saving your question scenes."
            });
        }
    }

    // Hvis du ønsker egen endpoint for å slette én question clienten sender inn
    // kan frontend heller fjerne den lokalt og så poste hele lista på nytt til /questions.
    // Derfor trenger vi ikke lenger DeleteQuestionInline i API-versjon.

    // =====================================================================
    // STEP 3: Ending Scenes + lagre Story i DB
    // =====================================================================

    [HttpGet("endings")]
    public IActionResult GetEndingScenes()
    {
        try
            {
                var sessionDto = HttpContext.Session.GetObject<StoryCreationDto>("StoryCreation");//skjekk den ut etterpå
                if (sessionDto == null)
                {
                    return BadRequest(new ErrorDto
                    {
                        ErrorTitle = "Session expired",
                        ErrorMessage = "Your session has expired. Please start creating your story again."
                    });
                }

                var dto = new EndingScenesDto
                {
                    GoodEnding = sessionDto.GoodEnding,
                    NeutralEnding = sessionDto.NeutralEnding,
                    BadEnding = sessionDto.BadEnding,
                    IsEditMode = false
                };

                return Ok(dto);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "[StoryCreationController -> GetEndingScenes] Failed to load endings from session");
                return StatusCode(500, new ErrorDto
                {
                    ErrorTitle = "Failed to load endings",
                    ErrorMessage = "An unexpected error occurred while loading your endings."
                });
            }
    }

    [HttpPost("endings")]
    public async Task<IActionResult> SaveEndingScenesAndCreateStory([FromBody] EndingScenesDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var sessionDto = HttpContext.Session.GetObject<StoryCreationDto>("StoryCreation");
            if (sessionDto == null)
            {
                return BadRequest(new ErrorDto
                {
                    ErrorTitle = "Session expired",
                    ErrorMessage = "Your session has expired. Please start creating your story again."
                });
            }

            // Oppdater endings i session
            sessionDto.GoodEnding = model.GoodEnding;
            sessionDto.NeutralEnding = model.NeutralEnding;
            sessionDto.BadEnding = model.BadEnding;
            HttpContext.Session.SetObject("StoryCreation", sessionDto);

            // Hent innlogget bruker
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized(new ErrorDto
                {
                    ErrorTitle = "Not logged in",
                    ErrorMessage = "You must be logged in to save a story."
                });
            }

            // Map QuestionScenes fra session til entities
            var questionScenes = sessionDto.QuestionScenes.Select(q => new QuestionScene
            {
                SceneText = q.StoryText,
                Question = q.QuestionText,
                AnswerOptions = q.Answers.Select((a, i) => new AnswerOption
                {
                    Answer = a.AnswerText,
                    FeedbackText = a.ContextText,
                    IsCorrect = i == q.CorrectAnswerIndex
                }).ToList()
            }).ToList();

            // Kjed sammen QuestionScenes i rekkefølge
            for (int i = 0; i < questionScenes.Count; i++)
            {
                questionScenes[i].NextQuestionScene = i < questionScenes.Count - 1
                    ? questionScenes[i + 1]
                    : null;
            }

            // Opprett Story
            var story = new Story
            {
                Title = sessionDto.Title,
                Description = sessionDto.Description,
                DifficultyLevel = sessionDto.DifficultyLevel!.Value,
                Accessibility = sessionDto.Accessibility!.Value,
                UserId = user.Id,
                Played = 0,
                Finished = 0,
                Failed = 0,
                Dnf = 0,
                IntroScene = new IntroScene
                {
                    IntroText = sessionDto.IntroText
                },
                QuestionScenes = questionScenes,
                EndingScenes = new List<EndingScene>
                {
                    new EndingScene { EndingType = EndingType.Good, EndingText = sessionDto.GoodEnding },
                    new EndingScene { EndingType = EndingType.Neutral, EndingText = sessionDto.NeutralEnding },
                    new EndingScene { EndingType = EndingType.Bad, EndingText = sessionDto.BadEnding }
                }
            };

            // Generer kode hvis privat
            if (sessionDto.Accessibility == Accessibility.Private)
            {
                story.Code = await GenerateUniqueStoryCodeAsync();
            }

            var success = await _storyRepository.AddFullStory(story);

            if (!success)
            {
                return StatusCode(500, new ErrorDto
                {
                    ErrorTitle = "Failed to save story",
                    ErrorMessage = "Something went wrong while saving your story."
                });
            }

            HttpContext.Session.Remove("StoryCreation");

            return Ok(new
            {
                message = "Your story has been successfully created.",
                storyId = story.StoryId
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[StoryCreationController -> SaveEndingScenesAndCreateStory] Failed to create story");
            return StatusCode(500, new ErrorDto
            {
                ErrorTitle = "Could not create story",
                ErrorMessage = "An unexpected error occurred while saving your story. Please try again."
            });
        }
    }

    // =====================================================================
    // Helper: Unique story code for private stories
    // =====================================================================

    private async Task<string> GenerateUniqueStoryCodeAsync()
    {
        string code;
        bool exists;
        do
        {
            code = Guid.NewGuid().ToString("N")[..8].ToUpper();
            exists = await _storyRepository.DoesCodeExist(code);
        }
        while (exists);

        return code;
    }
}