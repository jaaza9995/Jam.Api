using Jam.Api.DTOs.Story;
using Jam.Api.Models;

namespace Jam.Api.Services;

public interface IStoryCreationService
{
    Task<Story?> CreateStoryFromSessionAsync(StoryCreationSession session, AuthUser user);
}
