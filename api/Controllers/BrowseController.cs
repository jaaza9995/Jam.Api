using Jam.Api.DAL.StoryDAL;
using Jam.Api.Models;
using Jam.Api.DTOs.StoryPlaying;
using Jam.Api.DTOs.Shared;
using Jam.Api.DTOs.Story;
using Microsoft.AspNetCore.Mvc;

namespace Jam.Api.Controllers;

[ApiController]
[Route("api/browse")]
public class BrowseController : ControllerBase
{
    private readonly IStoryRepository _storyRepository;
    private readonly ILogger<BrowseController> _logger;

    public BrowseController(IStoryRepository storyRepository, ILogger<BrowseController> logger)
    {
        _storyRepository = storyRepository;
        _logger = logger;
    }

    // ---------------------------------------------------------------
    // GET PUBLIC AND PRIVATE STORIES (with optional search, THIS METHOD IS NOT IN USE)
    // ---------------------------------------------------------------
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
            Accessibility = s.Accessibility,
            DifficultyLevel = s.DifficultyLevel,
            Code = s.Code ?? ""
        };

    // ---------------------------------------------------------------
    // JOIN PRIVATE STORY (NOT IN USE)
    // ---------------------------------------------------------------
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

            var questionCount = await _storyRepository.GetAmountOfQuestionsForStory(story.StoryId) ?? 0;

            return Ok(new StoryCardDto
            {
                StoryId = story.StoryId,
                Title = story.Title,
                Description = story.Description,
                QuestionCount = questionCount,
                Accessibility = story.Accessibility,
                Code = story.Code ?? ""
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error joining private story");
            return StatusCode(500, new ErrorDto { ErrorTitle = "Failed to join story" });
        }
    }

    // ---------------------------------------------------------------
    // GET PUBLIC STORIES
    // ---------------------------------------------------------------
    [HttpGet("public")]
    public async Task<IActionResult> GetPublicGames()
    {
        try
        {
            var publicStories = await _storyRepository.GetAllPublicStories();
            var storyIds = publicStories.Select(s => s.StoryId).ToList();
            var questionCounts = await _storyRepository.GetQuestionCountsForStories(storyIds);

            var result = publicStories.Select(s => new StoryCardDto
            {
                StoryId = s.StoryId,
                Title = s.Title,
                Description = s.Description,
                DifficultyLevel = s.DifficultyLevel,
                Accessibility = s.Accessibility,
                QuestionCount = questionCounts.TryGetValue(s.StoryId, out var count) ? count : 0
            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[BrowseController -> GetPublicGames] Failed to fetch public stories");
            return StatusCode(500, new ErrorDto { ErrorTitle = "Failed to load public stories" });
        }
    }

    // ---------------------------------------------------------------
    // GET PRIVATE STORY (by code)
    // ---------------------------------------------------------------
    [HttpGet("private/{code}")]
    public async Task<IActionResult> GetPrivateGame(string code)
    {
        try
        {
            var story = await _storyRepository.GetPrivateStoryByCode(code);

            if (story == null)
                return NotFound(new { message = "No game found with this code." });

            var qCount = await _storyRepository.GetAmountOfQuestionsForStory(story.StoryId) ?? 0;

            return Ok(new
            {
                story.StoryId,
                story.Title,
                story.Description,
                story.DifficultyLevel,
                story.Accessibility,
                story.Code,
                QuestionCount = qCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[BrowseController -> GetPrivateGame] Failed to fetch private story for code {code}", code);
            return StatusCode(500, new ErrorDto { ErrorTitle = "Failed to load story" });
        }
    }
}
