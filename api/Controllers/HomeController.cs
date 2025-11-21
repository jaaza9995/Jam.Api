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
<<<<<<< HEAD
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
=======
//[Authorize]
public class HomeController : ControllerBase
{
    private readonly IStoryRepository _storyRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public HomeController(
        IStoryRepository storyRepository,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager
>>>>>>> 34f4b1e (CreationMode)
    )
    {
        _storyRepository = storyRepository;
        _userManager = userManager;
        _signInManager = signInManager;
    }


    // GET: api/home/homepage
    [HttpGet("homepage")]
    [AllowAnonymous]
    public async Task<IActionResult> GetHomePage()
    {
        // Bruk samme testbruker som story-creation logger på
        var user = await _userManager.FindByNameAsync("TheFlash");
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = "TheFlash",
                Firstname = "Flash",
                Lastname = "Allen",
                Email = "flash@test.com",
                EmailConfirmed = true
            };

<<<<<<< HEAD
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
                    s.Accessible
                }),
                RecentlyPlayed = recentlyPlayed.Select(s => new
                {
                    s.StoryId,
                    s.Title,
                    s.Description,
                    s.DifficultyLevel,
                    s.Accessible
                }),
                QuestionCounts = questionCounts
            });
=======
            var createRes = await _userManager.CreateAsync(user, "Test123!");
            if (!createRes.Succeeded)
                return StatusCode(500, new { message = "Failed to provision test user.", errors = createRes.Errors });
>>>>>>> 34f4b1e (CreationMode)
        }

        var userId = user.Id;

        var userStories = await _storyRepository.GetStoriesByUserId(userId);
        var recentlyPlayed = await _storyRepository.GetMostRecentPlayedStories(userId, 5);

        // Beregn spørsmålsantall
        var questionCounts = new Dictionary<int, int>();
        foreach (var s in userStories.Concat(recentlyPlayed))
        {
            questionCounts[s.StoryId] =
                await _storyRepository.GetAmountOfQuestionsForStory(s.StoryId) ?? 0;
        }

        return Ok(new
        {
            firstName = user.Firstname,  
            yourStories = userStories.Select(s => new
            {
                storyId = s.StoryId,
                title = s.Title,
                description = s.Description,
                difficultyLevel = s.DifficultyLevel.ToString(),
                accessibility = s.Accessibility.ToString(),
                questionCount = questionCounts[s.StoryId]
            }),
            recentlyPlayed = recentlyPlayed.Select(s => new
            {
                storyId = s.StoryId,
                title = s.Title,
                description = s.Description,
                difficultyLevel = s.DifficultyLevel.ToString(),
                accessibility = s.Accessibility.ToString(),
                questionCount = questionCounts[s.StoryId]
            })
        });
    }
}
