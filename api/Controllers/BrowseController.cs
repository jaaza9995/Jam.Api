using Jam.DAL.StoryDAL;
using Jam.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Jam.DTOs.JoinPrivateStoryRequestDto;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;


namespace Jam.Api.Controllers;

[ApiController]
[Route("api/browse")]
public class BrowseController : ControllerBase
{
    private readonly IStoryRepository _storyRepository;

    public BrowseController(IStoryRepository storyRepository)
    {
        _storyRepository = storyRepository;
    }

    // --------------------------
    // PUBLIC GAMES
    // --------------------------
    [HttpGet("public")]
    [Authorize]
    public async Task<IActionResult> GetPublicGames()
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
                s.DifficultyLevel,
                s.Accessibility,
                QuestionCount = qCount
            });
        }

        return Ok(result);
    }

    // --------------------------
    // PRIVATE GAME BY CODE
    // --------------------------
    [HttpGet("private/{code}")]
    [Authorize]
    public async Task<IActionResult> GetPrivateGame(string code)
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
}
