using Jam.Api.DAL.StoryDAL;
using Jam.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Jam.DTOs.JoinPrivateStoryRequestDto;
using Jam.DTOs;


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
            Accessibility = s.Accessibility.ToString(),
            DifficultyLevel = s.DifficultyLevel.ToString(), 
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

            // 🔥 Hent antall spørsmål fra databasen – IKKE fra Story.QuestionCount
            var questionCount = await _storyRepository.GetAmountOfQuestionsForStory(story.StoryId) ?? 0;

            return Ok(new StoryCardDto
            {
                StoryId = story.StoryId,
                Title = story.Title,
                Description = story.Description,
                QuestionCount = questionCount,  // 🔥 riktig nå!
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

    // --------------------------
    // PUBLIC GAMES
    // --------------------------
    [HttpGet("public")]
    [Authorize]
    public async Task<IActionResult> GetPublicGames()
    {
        try
        {
            var publicStories = await _storyRepository.GetAllPublicStories();

            var result = new List<object>();

            foreach (var s in publicStories)
            {
                var qCount = await _storyRepository.GetAmountOfQuestionsForStory(s.StoryId) ?? 0;

                result.Add(new
                {
                    s.StoryId,
                    s.Title,
                    s.Description,
                    DifficultyLevel = s.DifficultyLevel.ToString(),
                    Accessibility = s.Accessibility.ToString(),
                    QuestionCount = qCount
                });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[BrowseController -> GetPublicGames] Failed to fetch public stories");
            return StatusCode(500, new ErrorDto { ErrorTitle = "Failed to load public stories" });
        }
    }

    // --------------------------
    // PRIVATE GAME BY CODE
    // --------------------------
    [HttpGet("private/{code}")]
    [Authorize]
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
