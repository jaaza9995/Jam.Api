using System.Threading.Tasks;
using Jam.DAL.ApplicationUserDAL;
using Jam.DAL.PlayingSessionDAL;
using Jam.DAL.StoryDAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jam.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")] // Only admins can access
public class AdminController : ControllerBase
{
    private readonly IApplicationUserRepository _applicationUserRepository;
    private readonly IStoryRepository _storyRepository;
    private readonly IPlayingSessionRepository _playingSessionRepository;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IStoryRepository storyRepository,
        IApplicationUserRepository userRepository,
        IPlayingSessionRepository playingSessionRepository,
        ILogger<AdminController> logger
    )
    {
        _storyRepository = storyRepository;
        _applicationUserRepository = userRepository;
        _playingSessionRepository = playingSessionRepository;
        _logger = logger;
    }

    // ---------------- USERS ----------------
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        try
        {
            var users = await _applicationUserRepository.GetAllApplicationUsersAsync();
            return Ok(users);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while loading users list");
            return StatusCode(500, new { message = "Unexpected error while loading users list" });
        }
    }

    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            _logger.LogWarning("No id provided for deletion");
            return BadRequest(new { message = "User ID is required" });
        }

        try
        {
            var deleted = await _applicationUserRepository.DeleteApplicationUserAsync(id);
            if (!deleted)
                return NotFound(new { message = $"No user found with ID {id}" });

            return Ok(new { message = "User deleted successfully" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error deleting user with id {id}", id);
            return StatusCode(500, new { message = "Unexpected error deleting user" });
        }
    }

    // ---------------- STORIES ----------------
    [HttpGet("stories")]
    public async Task<IActionResult> GetStories()
    {
        try
        {
            var stories = await _storyRepository.GetAllStories();
            return Ok(stories);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while loading stories list");
            return StatusCode(500, new { message = "Unexpected error while loading stories list" });
        }
    }

    // ---------------- PLAYING SESSIONS ----------------
    [HttpGet("sessions")]
    public async Task<IActionResult> GetPlayingSessions()
    {
        try
        {
            var sessions = await _playingSessionRepository.GetAllPlayingSessions();
            return Ok(sessions);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while loading playing sessions list");
            return StatusCode(500, new { message = "Unexpected error while loading playing sessions list" });
        }
    }
}
