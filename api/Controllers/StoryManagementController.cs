using Jam.DAL.StoryDAL;
using Jam.DTOs;
using Jam.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Jam.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StoryManagementController : ControllerBase
{
    private readonly IStoryRepository _storyRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<StoryManagementController> _logger;

    public StoryManagementController(
        IStoryRepository storyRepository,
        UserManager<ApplicationUser> userManager,
        ILogger<StoryManagementController> logger)
    {
        _storyRepository = storyRepository;
        _userManager = userManager;
        _logger = logger;
    }

    // ============================================================
    // DELETE STORY  (admin or owner)
    // ============================================================
    [HttpDelete("{storyId:int}")]
    public async Task<IActionResult> DeleteStory(int storyId)
    {
        if (storyId <= 0)
            return BadRequest(new ErrorDto { ErrorTitle = "Invalid story ID." });

        try
        {
            var userId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");

            var story = await _storyRepository.GetStoryById(storyId);
            if (story == null)
                return NotFound(new ErrorDto { ErrorTitle = "Story not found." });

            if (story.UserId != userId && !isAdmin)
            {
                _logger.LogWarning("Unauthorized delete attempt for story {storyId}", storyId);
                return Forbid();
            }

            var deleted = await _storyRepository.DeleteStory(storyId);
            if (!deleted)
            {
                _logger.LogWarning("Failed to delete story {storyId}", storyId);
                return StatusCode(500, new ErrorDto { ErrorTitle = "Failed to delete story." });
            }

            return Ok(new { message = "Story deleted successfully." });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error deleting story {storyId}", storyId);
            return StatusCode(500, new ErrorDto
            {
                ErrorTitle = "Error deleting story",
                ErrorMessage = "An unexpected error occurred while deleting the story."
            });
        }
    }

    // ============================================================
    // GET STORY DETAILS  (admin or owner)
    // ============================================================
    [HttpGet("{storyId:int}")]
    public async Task<IActionResult> GetStoryDetails(int storyId)
    {
        if (storyId <= 0)
            return BadRequest(new ErrorDto { ErrorTitle = "Invalid story ID." });

        try
        {
            var userId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");

            var story = await _storyRepository.GetStoryById(storyId);
            if (story == null)
                return NotFound(new ErrorDto
                {
                    ErrorTitle = "Story not found",
                    ErrorMessage = "The story you requested could not be found."
                });

            if (story.UserId != userId && !isAdmin)
            {
                _logger.LogWarning("Unauthorized access to story {storyId} by user {userId}", storyId, userId);
                return Forbid();
            }

            return Ok(story);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error retrieving story details for {storyId}", storyId);
            return StatusCode(500, new ErrorDto
            {
                ErrorTitle = "Error loading story details",
                ErrorMessage = "An unexpected error occurred while loading the story details."
            });
        }
    }
}
