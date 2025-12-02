using Jam.Api.DAL.StoryDAL;
using Jam.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Jam.Api.Services;

public class AdminUserService : IAdminUserService
{
    private readonly UserManager<AuthUser> _userManager;
    private readonly IStoryRepository _storyRepository;
    private readonly ILogger<AdminUserService> _logger;

    public AdminUserService(
        UserManager<AuthUser> userManager,
        IStoryRepository storyRepository,
        ILogger<AdminUserService> logger
    )
    {
        _userManager = userManager;
        _storyRepository = storyRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<AuthUser>> GetAllUsersAsync()
    {
        try
        {
            return await _userManager.Users.ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[UserService -> GetAllUsersAsync] Error retrieving all users");
            return Enumerable.Empty<AuthUser>();
        }
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        try
        {
            // 1) Clear ownership via repository first to avoid FK issues
            await _storyRepository.ClearOwnershipForUserAsync(userId);

            // 2) Delete the identity user
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Failed to delete user {UserId}: {@Errors}", userId, result.Errors);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", userId);
            return false;
        }
    }
}