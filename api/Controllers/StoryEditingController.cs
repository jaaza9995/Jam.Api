using Jam.DAL.AnswerOptionDAL;
using Jam.DAL.SceneDAL;
using Jam.DAL.StoryDAL;
using Jam.DTOs;
using Jam.DTOs.StoryEditing;
using Jam.Models;
using Jam.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Jam.DAL;

namespace Jam.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoryEditingController : ControllerBase
{
    private readonly IStoryRepository _storyRepository;
    private readonly ISceneRepository _sceneRepository;
    private readonly IAnswerOptionRepository _answerOptionRepository;
    private readonly ILogger<StoryEditingController> _logger;

    public StoryEditingController(
        IStoryRepository storyRepository,
        ISceneRepository sceneRepository,
        IAnswerOptionRepository answerOptionRepository,
        ILogger<StoryEditingController> logger)
    {
        _storyRepository = storyRepository;
        _sceneRepository = sceneRepository;
        _answerOptionRepository = answerOptionRepository;
        _logger = logger;
    }

    // =====================================================================
    // EDIT STORY METADATA
    // =====================================================================

    [HttpGet("{storyId:int}")]
    public async Task<IActionResult> GetStoryForEdit(int storyId)
    {
        try
        {
            var story = await _storyRepository.GetStoryById(storyId);
            if (story == null)
                return NotFound(new ErrorDto { ErrorTitle = "Story not found" });

            var dto = new EditStoryDto
            {
                StoryId = story.StoryId,
                Title = story.Title,
                Description = story.Description,
                DifficultyLevel = story.DifficultyLevel,
                Accessibility = story.Accessibility
            };
            return Ok(dto);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to load story for editing");
            return StatusCode(500, new ErrorDto { ErrorTitle = "Unexpected error while loading story." });
        }
    }

    [HttpPut("{storyId:int}")]
    public async Task<IActionResult> UpdateStory(int storyId, [FromBody] EditStoryDto model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var story = await _storyRepository.GetStoryById(storyId);
            if (story == null)
                return NotFound(new ErrorDto { ErrorTitle = "Story not found" });

            // Oppdater felt
            story.Title = model.Title;
            story.Description = model.Description;
            story.DifficultyLevel = model.DifficultyLevel;

            if (story.Accessibility!= model.Accessibility)
            {
                if (model.Accessibility == Accessibility.Private)
                    story.Code = await GenerateUniqueStoryCodeAsync();
                else
                    story.Code = null;
            }

            story.Accessibility = model.Accessibility;

            var updated = await _storyRepository.UpdateStory(story);
            if (!updated)
                return StatusCode(500, new ErrorDto { ErrorTitle = "Failed to update story." });

            return Ok(new { message = "Story updated successfully." });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to update story metadata");
            return StatusCode(500, new ErrorDto { ErrorTitle = "Unexpected error while updating story." });
        }
    }

    private async Task<string> GenerateUniqueStoryCodeAsync()
    {
        string code;
        bool exists;
        do
        {
            code = Guid.NewGuid().ToString("N")[..8].ToUpper();
            exists = await _storyRepository.DoesCodeExist(code);
        } while (exists);
        return code;
    }

    // =====================================================================
    // EDIT INTRO SCENE
    // =====================================================================

    [HttpGet("{storyId:int}/intro")]
    public async Task<IActionResult> GetIntroScene(int storyId)
    {
        try
        {
            var introScene = await _sceneRepository.GetIntroSceneByStoryId(storyId);
            if (introScene == null)
                return NotFound(new ErrorDto { ErrorTitle = "Intro scene not found" });

            return Ok(new EditIntroSceneDto
            {
                StoryId = introScene.StoryId,
                IntroText = introScene.IntroText
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to load intro scene");
            return StatusCode(500, new ErrorDto { ErrorTitle = "Unexpected error while loading intro scene." });
        }
    }

    [HttpPut("{storyId:int}/intro")]
    public async Task<IActionResult> UpdateIntroScene(int storyId, [FromBody] EditIntroSceneDto model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var introScene = await _sceneRepository.GetIntroSceneByStoryId(storyId);
            if (introScene == null)
                return NotFound(new ErrorDto { ErrorTitle = "Intro scene not found" });

            introScene.IntroText = model.IntroText;
            var updated = await _sceneRepository.UpdateIntroScene(introScene);

            if (!updated)
                return StatusCode(500, new ErrorDto { ErrorTitle = "Failed to update intro scene." });

            return Ok(new { message = "Intro scene updated successfully." });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error updating intro scene");
            return StatusCode(500, new ErrorDto { ErrorTitle = "Unexpected error while updating intro scene." });
        }
    }

    // =====================================================================
    // EDIT QUESTION SCENES
    // =====================================================================

    [HttpGet("{storyId:int}/questions")]
    public async Task<IActionResult> GetQuestionScenes(int storyId, [FromServices] StoryDbContext db)
    {
        try
        {
            var questionScenes = await db.QuestionScenes
                .Include(q => q.AnswerOptions)
                .Where(q => q.StoryId == storyId)
                .OrderBy(q => q.QuestionSceneId)
                .ToListAsync();

            if (!questionScenes.Any())
                return NotFound(new ErrorDto { ErrorTitle = "No question scenes found." });

            var dtoList = questionScenes.Select(scene => new QuestionSceneBaseDto
            {
                QuestionSceneId = scene.QuestionSceneId,
                StoryId = scene.StoryId,
                StoryText = scene.SceneText,
                QuestionText = scene.Question,
                Answers = scene.AnswerOptions.Select(a => new AnswerOptionInput
                {
                    AnswerText = a.Answer,
                    ContextText = a.FeedbackText
                }).ToList(),
                CorrectAnswerIndex = scene.AnswerOptions.FindIndex(a => a.IsCorrect),
                IsEditing = true
            }).ToList();

            return Ok(dtoList);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to load question scenes");
            return StatusCode(500, new ErrorDto { ErrorTitle = "Unexpected error while loading question scenes." });
        }
    }

    [HttpPut("{storyId:int}/questions")]
    public async Task<IActionResult> UpdateQuestionScenes(int storyId, [FromBody] List<QuestionSceneBaseDto> model)
    {
        var firstModelError = ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .FirstOrDefault() ?? "Invalid payload.";

        _logger.LogWarning("UpdateQuestionScenes modelstate invalid: {Error}", firstModelError);
        return BadRequest(new ErrorDto
        {
            ErrorTitle = "Validation failed",
            ErrorMessage = firstModelError
        });
    }

    // Normalize incoming answers so every scene has exactly 4 slots (pad/trim)
    foreach (var scene in model)
    {
        scene.Answers = NormalizeAnswers(scene.Answers);
        // Guard against out-of-range values from the client (clamp to 0-3)
        if (scene.CorrectAnswerIndex < 0) scene.CorrectAnswerIndex = 0;
        else if (scene.CorrectAnswerIndex > 3) scene.CorrectAnswerIndex = 3;

        // Trim all answer texts to avoid false "empty" hits
        scene.Answers = scene.Answers
            .Select(a => new AnswerOptionInput
            {
                AnswerText = (a.AnswerText ?? string.Empty).Trim(),
                ContextText = (a.ContextText ?? string.Empty).Trim()
            })
            .ToList();
    }

    var validationError = ValidateQuestionScenes(model);
    if (!string.IsNullOrEmpty(validationError))
    {
        _logger.LogWarning("UpdateQuestionScenes validation failed: {Error}", validationError);
        return BadRequest(new ErrorDto
        {
            ErrorTitle = "Validation failed",
            ErrorMessage = validationError
        });
    }

    try
    {
        // Hent alle eksisterende spørsmål for story
        var existingScenes = await db.QuestionScenes
            .Include(q => q.AnswerOptions)
            .Where(q => q.StoryId == storyId)
            .ToListAsync();

        var activeScenes = model.Where(m => !m.MarkedForDeletion).ToList();

        // --- SLETT SPØRSMÅL SOM IKKE LENGER FINNES ---
        var idsFromClient = activeScenes
            .Where(m => m.QuestionSceneId != 0)
            .Select(m => m.QuestionSceneId)
            .ToHashSet();

        var scenesToDelete = existingScenes
            .Where(s => !idsFromClient.Contains(s.QuestionSceneId))
            .ToList();

        db.QuestionScenes.RemoveRange(scenesToDelete);

        // --- OPPDATER EKSISTERENDE ---
        foreach (var dto in activeScenes.Where(m => m.QuestionSceneId != 0))
        {
            var scene = existingScenes
                .First(s => s.QuestionSceneId == dto.QuestionSceneId);

            scene.SceneText = dto.StoryText;
            scene.Question = dto.QuestionText;

            // Fjern gamle svar og legg inn de nye
            scene.AnswerOptions.Clear();

            scene.AnswerOptions = dto.Answers
                .Select((a, idx) => new AnswerOption
                {
                    AnswerOptionId = a.AnswerOptionId, 
                    Answer = a.AnswerText,
                    FeedbackText = a.ContextText,
                    IsCorrect = idx == dto.CorrectAnswerIndex,
                    QuestionSceneId = scene.QuestionSceneId
                })
                .ToList();
        }

        // --- LEGG TIL NYE SPØRSMÅL (QuestionSceneId == 0) ---
        foreach (var dto in activeScenes.Where(m => m.QuestionSceneId == 0))
        {
            var newScene = new QuestionScene
            {
                StoryId = storyId,
                SceneText = dto.StoryText,
                Question = dto.QuestionText,
                AnswerOptions = dto.Answers
                    .Select((a, idx) => new AnswerOption
                    {
                        Answer = a.AnswerText,
                        FeedbackText = a.ContextText,
                        IsCorrect = i == vm.CorrectAnswerIndex
                    }).ToList()
                }).ToList();

            var success = await _sceneRepository.UpdateQuestionScenes(questionScenes);
            if (!success)
                return StatusCode(500, new ErrorDto { ErrorTitle = "Failed to update question scenes." });

            return Ok(new { message = "Question scenes updated successfully." });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error updating question scenes");
            return StatusCode(500, new ErrorDto { ErrorTitle = "Unexpected error while updating question scenes." });
        }
    }

    [HttpDelete("questions/{questionSceneId:int}")]
    public async Task<IActionResult> DeleteQuestion(int questionSceneId)
    {
        try
        {
            var success = await _sceneRepository.DeleteQuestionScene(questionSceneId);
            if (!success)
                return StatusCode(500, new ErrorDto { ErrorTitle = "Failed to delete question." });

            return Ok(new { message = "Question deleted successfully." });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error deleting question scene");
            return StatusCode(500, new ErrorDto { ErrorTitle = "Unexpected error while deleting question scene." });
        }
    }

    // =====================================================================
    // EDIT ENDING SCENES
    // =====================================================================

    [HttpGet("{storyId:int}/endings")]
    public async Task<IActionResult> GetEndingScenes(int storyId)
    {
        try
        {
            var endings = await _sceneRepository.GetEndingScenesByStoryId(storyId);
            if (!endings.Any())
                return NotFound(new ErrorDto { ErrorTitle = "Ending scenes not found." });

            var dict = endings.ToDictionary(e => e.EndingType, e => e.EndingText);
            var dto = new EndingScenesDto
            {
                StoryId = storyId,
                GoodEnding = dict.GetValueOrDefault(EndingType.Good, ""),
                NeutralEnding = dict.GetValueOrDefault(EndingType.Neutral, ""),
                BadEnding = dict.GetValueOrDefault(EndingType.Bad, "")
            };
            return Ok(dto);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to load ending scenes");
            return StatusCode(500, new ErrorDto { ErrorTitle = "Unexpected error while loading ending scenes." });
        }
    }

    [HttpPut("{storyId:int}/endings")]
    public async Task<IActionResult> UpdateEndingScenes(int storyId, [FromBody] EndingScenesDto model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var endings = await _sceneRepository.GetEndingScenesByStoryId(storyId);
            if (!endings.Any())
                return NotFound(new ErrorDto { ErrorTitle = "Ending scenes not found." });

            foreach (var e in endings)
            {
                e.EndingText = e.EndingType switch
                {
                    EndingType.Good => model.GoodEnding,
                    EndingType.Neutral => model.NeutralEnding,
                    EndingType.Bad => model.BadEnding,
                    _ => e.EndingText
                };
            }

            var success = await _sceneRepository.UpdateEndingScenes(endings);
            if (!success)
                return StatusCode(500, new ErrorDto { ErrorTitle = "Failed to update ending scenes." });

            return Ok(new { message = "Ending scenes updated successfully." });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error updating ending scenes");
            return StatusCode(500, new ErrorDto { ErrorTitle = "Unexpected error while updating ending scenes." });
        }
    }
    [HttpDelete("{storyId:int}")]
    public async Task<IActionResult> DeleteStory(int storyId)
    {
        try
        {
            var success = await _storyRepository.DeleteStory(storyId);

            if (!success)
                return StatusCode(500, new ErrorDto { ErrorTitle = "Failed to delete story." });

            return Ok(new { message = "Story deleted successfully." });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error deleting story");
            return StatusCode(500, new ErrorDto { ErrorTitle = "Unexpected error while deleting story." });
        }
    }

    // Simple validation to avoid saving empty questions/answers from the editor
    private string? ValidateQuestionScenes(List<QuestionSceneBaseDto> model)
    {
        var activeScenes = model.Where(m => !m.MarkedForDeletion).ToList();
        for (int i = 0; i < activeScenes.Count; i++)
        {
            var scene = activeScenes[i];
            // Answers are normalized to 4 entries before validation
            if (string.IsNullOrWhiteSpace(scene.StoryText))
                return "Story context cannot be empty.";

            if (string.IsNullOrWhiteSpace(scene.QuestionText))
                return "Question text cannot be empty.";

            // Answers are normalized to 4 items; we do not block on empty answer/context here

            if (scene.CorrectAnswerIndex is < 0 or > 3)
                return "A correct answer must be selected.";
        }

        return null;
    }

    // Ensure answers list always has exactly 4 elements
    private List<AnswerOptionInput> NormalizeAnswers(List<AnswerOptionInput>? answers)
    {
        var list = answers?.Take(4).ToList() ?? new List<AnswerOptionInput>();
        while (list.Count < 4)
            list.Add(new AnswerOptionInput());
        return list;
    }
}
