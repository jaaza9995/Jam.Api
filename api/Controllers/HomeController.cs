using Jam.DAL.StoryDAL;
using Jam.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("Invalid or expired authentication token.");
                await _signInManager.SignOutAsync();
                return Unauthorized(new { message = "User session expired. Please log in again." });
            }

            var userStories = await _storyRepository.GetStoriesByUserId(user.Id);
            var recentlyPlayed = await _storyRepository.GetMostRecentPlayedStories(user.Id, 5);

            var questionCounts = new Dictionary<int, int>();
            foreach (var s in userStories.Concat(recentlyPlayed))
            {
                questionCounts[s.StoryId] = await _storyRepository.GetAmountOfQuestionsForStory(s.StoryId) ?? 0;
            }

            // Returner kun rene data (ingen ViewModels)
            return Ok(new
            {
                FirstName = user.UserName,
                YourStories = userStories.Select(s => new
                {
                    s.StoryId,
                    s.Title,
                    s.Description,
                    s.DifficultyLevel,
                    s.Accessibility
                }),
                RecentlyPlayed = recentlyPlayed.Select(s => new
                {
                    s.StoryId,
                    s.Title,
                    s.Description,
                    s.DifficultyLevel,
                    s.Accessibility
                }),
                QuestionCounts = questionCounts
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while loading dashboard");
            return StatusCode(500, new { message = "Unexpected error while loading dashboard" });
        }
    }
}