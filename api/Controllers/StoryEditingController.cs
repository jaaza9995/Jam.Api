using Jam.Api.DAL.SceneDAL;
using Jam.Api.DAL.StoryDAL;
using Jam.Api.DTOs.Shared;
using Jam.Api.DTOs.IntroScenes;
using Jam.Api.DTOs.QuestionScenes;
using Jam.Api.DTOs.EndingScenes;
using Jam.Api.DTOs.Story;
using Jam.Api.Models;
using Jam.Api.Models.Enums;
using Jam.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Jam.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoryEditingController : ControllerBase
{
    private readonly IStoryRepository _storyRepository;
    private readonly ISceneRepository _sceneRepository;
    private readonly ILogger<StoryEditingController> _logger;
    private readonly IStoryCodeService _codeService;
    private readonly IStoryEditingService _storyEditingService;

    public StoryEditingController(
        IStoryRepository repo,
        ISceneRepository sceneRepository,
        IStoryCodeService codeService,
        IStoryEditingService storyEditingService,
        ILogger<StoryEditingController> logger
    )
    {
        _storyRepository = repo;
        _sceneRepository = sceneRepository;
        _codeService = codeService;
        _storyEditingService = storyEditingService;
        _logger = logger;
    }

    // ---------------------------------------------------------------
    // STORY METADATA
    // ---------------------------------------------------------------

    [HttpGet("{storyId:int}")]
    public async Task<IActionResult> GetStoryForEdit(int storyId)
    {
        try
        {
            var story = await _storyRepository.GetStoryById(storyId);
            if (story == null)
                return NotFound(new ErrorDto { ErrorTitle = "Story not found." });

            var questionCount = await _storyRepository.GetAmountOfQuestionsForStory(storyId) ?? 0;

            return Ok(new EditStoryDto
            {
                StoryId = story.StoryId,
                Title = story.Title,
                Description = story.Description,
                DifficultyLevel = story.DifficultyLevel,
                Accessibility = story.Accessibility,
                Code = story.Code,
                QuestionCount = questionCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[StoryEditingController -> GetStoryForEdit] Failed for storyId {storyId}", storyId);
            return StatusCode(500, new ErrorDto { ErrorTitle = "Failed to load story." });
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
                return NotFound(new ErrorDto { ErrorTitle = "Story not found." });

            story.Title = model.Title;
            story.Description = model.Description;
            story.DifficultyLevel = model.DifficultyLevel;

            // If accessibility changed:
            if (story.Accessibility != model.Accessibility)
            {
                story.Code = (model.Accessibility == Accessibility.Private)
                    ? await _codeService.GenerateUniqueStoryCodeAsync()
                    : null;
            }

            story.Accessibility = model.Accessibility;

            if (!await _storyRepository.UpdateStory(story))
                return StatusCode(500, new ErrorDto { ErrorTitle = "Failed to update story metadata." });

            return Ok(new { message = "Story updated successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[StoryEditingController -> UpdateStory] Failed for storyId {storyId}", storyId);
            return StatusCode(500, new ErrorDto { ErrorTitle = "Failed to update story metadata." });
        }
    }



    // ---------------------------------------------------------------
    // INTRO SCENE
    // ---------------------------------------------------------------

    [HttpGet("{storyId:int}/intro")]
    public async Task<IActionResult> GetIntro(int storyId)
    {
        var intro = await _sceneRepository.GetIntroSceneByStoryId(storyId);
        if (intro == null)
            return NotFound(new ErrorDto { ErrorTitle = "Intro scene not found." });

        return Ok(new IntroSceneDto
        {
            StoryId = intro.StoryId,
            IntroText = intro.IntroText
        });
    }

    [HttpPut("{storyId:int}/intro")]
    public async Task<IActionResult> UpdateIntro(int storyId, [FromBody] IntroSceneDto model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var intro = await _sceneRepository.GetIntroSceneByStoryId(storyId);
        if (intro == null)
            return NotFound(new ErrorDto { ErrorTitle = "Intro scene not found." });

        intro.IntroText = model.IntroText;

        if (!await _sceneRepository.UpdateIntroScene(intro))
            return StatusCode(500, new ErrorDto { ErrorTitle = "Failed to update intro scene." });

        return Ok(new { message = "Intro updated successfully." });
    }



    // ---------------------------------------------------------------
    // QUESTION SCENES
    // ---------------------------------------------------------------

    [HttpGet("{storyId:int}/questions")]
    public async Task<IActionResult> GetQuestions(int storyId)
    {
        var scenes = await _sceneRepository.GetQuestionScenesByStoryId(storyId);
        if (scenes == null || !scenes.Any())
            return NotFound(new ErrorDto { ErrorTitle = "No question scenes found." });

        var dto = scenes.Select(scene => new QuestionSceneDto
        {
            QuestionSceneId = scene.QuestionSceneId,
            StoryText = scene.SceneText,
            QuestionText = scene.Question,
            CorrectAnswerIndex = scene.AnswerOptions.FindIndex(a => a.IsCorrect),

            Answers = scene.AnswerOptions.Select(a => new AnswerOptionDto
            {
                AnswerOptionId = a.AnswerOptionId,
                AnswerText = a.Answer,
                ContextText = a.FeedbackText
            }).ToList()
        }).ToList();

        return Ok(dto);
    }

    [HttpPut("{storyId:int}/questions")]
    public async Task<IActionResult> UpdateQuestions(
        int storyId,
        [FromBody] List<UpdateQuestionSceneDto> model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            // Calls the service layer which takes care of updating
            var result = await _storyEditingService.UpdateQuestionScenesAsync(storyId, model);
            if (!result)
            {
                return StatusCode(500, new ErrorDto { ErrorTitle = "Failed to update question scenes." });
            }

            return Ok(new { message = "Questions updated successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[StoryEditingController -> UpdateQuestions] Failed for storyId {storyId}", storyId);
            return StatusCode(500, new ErrorDto { ErrorTitle = "Failed to update question scenes." });
        }
    }



    // ---------------------------------------------------------------
    // ENDING SCENES
    // ---------------------------------------------------------------

    [HttpGet("{storyId:int}/endings")]
    public async Task<IActionResult> GetEndings(int storyId)
    {
        var list = await _sceneRepository.GetEndingScenesByStoryId(storyId);
        if (list == null || !list.Any())
            return NotFound(new ErrorDto { ErrorTitle = "Ending scenes not found." });

        var dto = new UpdateEndingSceneDto
        {
            StoryId = storyId,
            GoodEnding = list.FirstOrDefault(e => e.EndingType == EndingType.Good)?.EndingText ?? "",
            NeutralEnding = list.FirstOrDefault(e => e.EndingType == EndingType.Neutral)?.EndingText ?? "",
            BadEnding = list.FirstOrDefault(e => e.EndingType == EndingType.Bad)?.EndingText ?? ""
        };

        return Ok(dto);
    }

    [HttpPut("{storyId:int}/endings")]
    public async Task<IActionResult> UpdateEndings(int storyId, [FromBody] UpdateEndingSceneDto model)
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

            if (!await _sceneRepository.UpdateEndingScenes(endings))
                return StatusCode(500, new ErrorDto { ErrorTitle = "Failed to update endings." });

            return Ok(new { message = "Endings updated successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[StoryEditingController -> UpdateEndings] Failed for storyId {storyId}", storyId);
            return StatusCode(500, new ErrorDto { ErrorTitle = "Failed to update endings." });
        }
    }



    // ---------------------------------------------------------------
    // DELETE STORY
    // ---------------------------------------------------------------

    [HttpDelete("{storyId:int}")]
    public async Task<IActionResult> DeleteStory(int storyId)
    {
        try
        {
            if (!await _storyRepository.DeleteStory(storyId))
                return StatusCode(500, new ErrorDto { ErrorTitle = "Failed to delete story." });

            return Ok(new { message = "Story deleted successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[StoryEditingController -> DeleteStory] Failed for storyId {storyId}", storyId);
            return StatusCode(500, new ErrorDto { ErrorTitle = "Failed to delete story." });
        }
    }
}
