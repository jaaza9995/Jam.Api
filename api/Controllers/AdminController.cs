using Jam.Api.DAL.StoryDAL;
using Jam.Api.DTOs.Admin;
using Jam.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Jam.Api.Services;


namespace Jam.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")] // Only admins can access
public class AdminController : ControllerBase
{
    private readonly IStoryRepository _storyRepository;
    private readonly UserManager<AuthUser> _userManager;
    private readonly IAdminUserService _adminUserService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IStoryRepository storyRepository,
        UserManager<AuthUser> userManager,
        IAdminUserService adminUserService,
        ILogger<AdminController> logger
    )
    {
        _storyRepository = storyRepository;
        _userManager = userManager;
        _adminUserService = adminUserService;
        _logger = logger;
    }


    // ---------------------------------------------------------------
    // GET USERS
    // ---------------------------------------------------------------

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        try
        {
            var users = await _adminUserService.GetAllUsersAsync();

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


    // ---------------------------------------------------------------
    // DELETE USER
    // ---------------------------------------------------------------

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
            // 1) Prevent deleting yourself (admin)
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(currentUserId) && currentUserId == id)
            {
                _logger.LogWarning("[AdminController -> DeleteUser] Admin attempted to delete their own account ({userId})", id);
                return BadRequest(new { message = "You cannot delete your own account." });
            }

            // 2) Ensure target user exists
            var targetUser = await _userManager.FindByIdAsync(id);
            if (targetUser == null)
            {
                _logger.LogWarning("[AdminController -> DeleteUser] No user found with ID {userId}", id);
                return NotFound(new { message = $"No user found with ID {id}" });
            }

            // 3) Prevent deleting another admin (only one admin account, but nice to have the check)
            if (await _userManager.IsInRoleAsync(targetUser, "Admin"))
            {
                _logger.LogWarning("[AdminController -> DeleteUser] Attempt to delete Admin user {userId} blocked", id);
                return BadRequest(new { message = "Cannot delete a user in the Admin role." });
            }

            // 4) Proceed with deletion (using AdminUserService)
            var deleted = await _adminUserService.DeleteUserAsync(id);
            if (!deleted)
            {
                _logger.LogWarning("[AdminController -> DeleteUser] Deletion failed for user ID {userId}", id);
                return StatusCode(500, new { message = "Failed to delete user" });
            }

            return NoContent();
        }

        catch (Exception e)
        {
            _logger.LogError(e, "[AdminController -> DeleteUser] Unexpected error while deleting user with ID {userId}", id);
            return StatusCode(500, new { message = "Unexpected error deleting user" });
        }
    }




    // ---------------------------------------------------------------
    // GET STORIES
    // ---------------------------------------------------------------

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


    // ---------------------------------------------------------------
    // DELETE STORY
    // ---------------------------------------------------------------

    [HttpDelete("stories/{id}")]
    public async Task<IActionResult> DeleteStory(int id)
    {
        try
        {
            var deleted = await _storyRepository.DeleteStory(id);
            if (!deleted)
            {
                _logger.LogWarning("Failed to delete story with ID {StoryId}", id);
                return StatusCode(500, new { message = "Failed to delete story" });
            }
            return NoContent();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error while deleting story with ID {StoryId}", id);
            return StatusCode(500, new { message = "Unexpected error deleting story" });
        }
    }
}