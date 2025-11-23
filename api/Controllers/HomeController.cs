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
[Route("api/[controller]")]
public class HomeController : ControllerBase
{
    private readonly IStoryRepository _storyRepository;
    private readonly UserManager<AuthUser> _userManager;
    private readonly SignInManager<AuthUser> _signInManager;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        IStoryRepository storyRepository,
        UserManager<AuthUser> userManager,
        SignInManager<AuthUser> signInManager,
        ILogger<HomeController> logger
    )
    {
        _storyRepository = storyRepository;
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    // GET: api/home/homepage
  [HttpGet("homepage")]
    public async Task<IActionResult> GetHomePage()
    {
        try
        {
            // Hent UserId direkte fra JWT
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Token missing user ID.");
                return Unauthorized(new { message = "Invalid token" });
            }

            // Hent bruker basert på ID (IKKE email)
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("User not found for ID {id}", userId);
                await _signInManager.SignOutAsync();
                return Unauthorized(new { message = "User session expired. Please log in again." });
            }

            var userStories = await _storyRepository.GetStoriesByUserId(user.Id);
        var recentlyPlayed = (await _storyRepository.GetMostRecentPlayedStories(user.Id, 5))
            .OrderByDescending(s => s.Played)   // hvis Played brukes
            .ToList();


            var questionCounts = new Dictionary<int, int>();
            foreach (var s in userStories.Concat(recentlyPlayed))
            {
                questionCounts[s.StoryId] = await _storyRepository.GetAmountOfQuestionsForStory(s.StoryId) ?? 0;
            }

            return Ok(new
            {
                FirstName = user.UserName,
                YourStories = userStories.Select(s => new
                {
                    s.StoryId,
                    s.Title,
                    s.Description,
                    s.DifficultyLevel,
                    s.Accessibility,
                    s.Code,
                    QuestionCount = questionCounts[s.StoryId],
                    
                }),
                RecentlyPlayed = recentlyPlayed.Select(s => new
                {
                    s.StoryId,
                    s.Title,
                    s.Description,
                    s.DifficultyLevel,
                    s.Accessibility,
                    s.Code,
                    QuestionCount = questionCounts[s.StoryId]
                
                })
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while loading dashboard");
            return StatusCode(500, new { message = "Unexpected error while loading dashboard" });
        }
    }
}