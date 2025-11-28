using Jam.Api.Models;

namespace Jam.Api.Services;

public interface IAdminUserService
{
    Task<IEnumerable<AuthUser>> GetAllUsersAsync();
    Task<bool> DeleteUserAsync(string userId);
}