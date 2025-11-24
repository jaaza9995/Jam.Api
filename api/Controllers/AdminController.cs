using Jam.Api.DAL;
using Jam.Api.DAL.PlayingSessionDAL;
using Jam.Api.DAL.StoryDAL;
using Jam.Api.DTOs.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jam.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")] // Only admins can access
public class AdminController : ControllerBase
{
    private readonly UserService _userService;
    private readonly IStoryRepository _storyRepository;

    private readonly IPlayingSessionRepository _playingSessionRepository;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IStoryRepository storyRepository,
        UserService userService,
        IPlayingSessionRepository playingSessionRepository,
        ILogger<AdminController> logger
    )
    {
        _storyRepository = storyRepository;
        _userService = userService;
        _playingSessionRepository = playingSessionRepository;
        _logger = logger;
    }

    // ---------------- USERS ----------------
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        try
        {
            var users = await _userService.GetAllUsersAsync();

            // Mapping AuthUser to UserDto
            // Not including user attributes like password hash for security reasons
            var userData = users.Select(u => new UserDataDto
            {
                Id = u.Id,
                UserName = u.UserName ?? string.Empty,
                Email = u.Email ?? string.Empty,
            }).ToList();

            return Ok(userData);
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
            _logger.LogWarning("[AdminController -> DeleteUser] No id provided for deletion");
            return BadRequest(new { message = "User ID is required" });
        }

        try
        {
            var deleted = await _userService.DeleteUserAsync(id);

            if (!deleted)
                _logger.LogWarning("[AdminController -> DeleteUser] No user found with ID {userId}", id);
            return NotFound(new { message = $"No user found with ID {id}" });
        }

        catch (Exception e)
        {
            _logger.LogError(e, "[AdminController -> DeleteUser] Unexpected error while deleting user with ID {userId}", id);
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

            var storyData = stories.Select(s => new StoryDataDto
            {
                Id = s.StoryId,
                Title = s.Title,
                Accessibility = s.Accessibility,
                UserId = s.UserId ?? string.Empty,
            }).ToList();
            return Ok(storyData);
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