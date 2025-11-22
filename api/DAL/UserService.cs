using Jam.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Jam.DAL
{
    public class UserService
    {
        private readonly UserManager<AuthUser> _userManager;
        private readonly ILogger<UserService> _logger;

        public UserService(UserManager<AuthUser> userManager, ILogger<UserService> logger)
        {
            _userManager = userManager;
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
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("[UserService -> DeleteUserAsync] Invalid userId {userId} provided", userId);
                return false;
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("[UserService -> DeleteUserAsync] No user found with id {userId} to delete", userId);
                    return false;
                }

                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("[UserService -> DeleteUserAsync] Failed to delete user {userId}. Errors: {errors}", userId, errors);
                }
                else
                {
                    _logger.LogInformation("[UserService -> DeleteUserAsync] Successfully deleted user with id {userId}", userId);
                }

                return result.Succeeded;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "[UserService -> DeleteUserAsync] Error deleting user with id {userId}", userId);
                return false;
            }
        }
    }
}
