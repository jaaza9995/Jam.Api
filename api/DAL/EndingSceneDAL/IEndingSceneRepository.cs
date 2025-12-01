using Jam.Api.Models;

namespace Jam.Api.DAL.EndingSceneDAL;

public interface IEndingSceneRepository
{
    Task<IEnumerable<EndingScene>> GetEndingScenesByStoryId(int storyId);
    Task<EndingScene?> GetEndingSceneById(int endingSceneId);
    Task<EndingScene?> GetGoodEndingSceneByStoryId(int storyId);
    Task<EndingScene?> GetNeutralEndingSceneByStoryId(int storyId);
    Task<EndingScene?> GetBadEndingSceneByStoryId(int storyId);
    Task<bool> AddEndingScene(EndingScene endingScene);
    Task<bool> UpdateEndingScene(EndingScene EndingScene);
    Task<bool> UpdateEndingScenes(IEnumerable<EndingScene> endingScenes); 
    Task<bool> DeleteEndingScene(int endingSceneId);
}