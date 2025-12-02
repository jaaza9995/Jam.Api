using Jam.Api.Models;

namespace Jam.Api.DAL.EndingSceneDAL;

/// <summary>
/// Method: AddEndingScene(), UpdateEndingScene(), and
/// DeleteEndingScene() is not in use.
/// Retained for CRUD completeness and future standalone operations.
/// </summary>
public interface IEndingSceneRepository
{
    // ----------------------- Get / Read -----------------------
    Task<IEnumerable<EndingScene>> GetEndingScenesByStoryId(int storyId);
    Task<EndingScene?> GetEndingSceneById(int endingSceneId);
    Task<EndingScene?> GetGoodEndingSceneByStoryId(int storyId);
    Task<EndingScene?> GetNeutralEndingSceneByStoryId(int storyId);
    Task<EndingScene?> GetBadEndingSceneByStoryId(int storyId);


    // ----------------------- Create -----------------------
    Task<bool> AddEndingScene(EndingScene endingScene);


    // ----------------------- Update -----------------------
    Task<bool> UpdateEndingScene(EndingScene EndingScene);
    Task<bool> UpdateEndingScenes(IEnumerable<EndingScene> endingScenes); 


    // ----------------------- Delete -----------------------
    Task<bool> DeleteEndingScene(int endingSceneId);
}