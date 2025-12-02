using Jam.Api.Models;
using Jam.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Jam.Api.DAL.EndingSceneDAL;

public class EndingSceneRepository : IEndingSceneRepository
{
    private readonly StoryDbContext _db;
    private readonly ILogger<EndingSceneRepository> _logger;

    public EndingSceneRepository(
        StoryDbContext db,
        ILogger<EndingSceneRepository> logger
    )
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IEnumerable<EndingScene>> GetEndingScenesByStoryId(int storyId)
    {
        if (storyId <= 0)
        {
            _logger.LogWarning("[SceneRepository -> GetEndingScenesByStoryId] Invalid storyId provided: {storyId}", storyId);
            return Enumerable.Empty<EndingScene>(); // not returning null to avoid null reference exceptions
        }

        try
        {
            return await _db.EndingScenes
                .AsNoTracking()
                .Where(e => e.StoryId == storyId)
                .ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[SceneRepository -> GetEndingScenesByStoryId] Error retrieving EndingScenes for storyId {storyId}", storyId);
            return Enumerable.Empty<EndingScene>(); // not returning null to avoid null reference exceptions
        }
    }

    public async Task<EndingScene?> GetEndingSceneById(int endingSceneId)
    {
        if (endingSceneId <= 0)
        {
            _logger.LogWarning("[SceneRepository -> GetEndingSceneById] Invalid EndingSceneId provided: {endingSceneId}", endingSceneId);
            return null;
        }

        try
        {
            var ending = await _db.EndingScenes
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EndingSceneId == endingSceneId);

            if (ending == null)
            {
                _logger.LogWarning("[SceneRepository -> GetEndingSceneById] No EndingScene found with id {endingSceneIdid}", endingSceneId);
            }

            return ending;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[SceneRepository -> GetEndingSceneById] Error retrieving EndingScene with id {endingSceneId}", endingSceneId);
            return null;
        }
    }

    public async Task<EndingScene?> GetGoodEndingSceneByStoryId(int storyId)
    {
        return await GetEndingSceneByTypeAsync(storyId, EndingType.Good, "Good");
    }

    public async Task<EndingScene?> GetNeutralEndingSceneByStoryId(int storyId)
    {
        return await GetEndingSceneByTypeAsync(storyId, EndingType.Neutral, "Neutral");
    }

    public async Task<EndingScene?> GetBadEndingSceneByStoryId(int storyId)
    {
        return await GetEndingSceneByTypeAsync(storyId, EndingType.Bad, "Bad");
    }

    // private method to reduce code duplication
    private async Task<EndingScene?> GetEndingSceneByTypeAsync(int storyId, EndingType type, string typeName)
    {
        if (storyId <= 0)
        {
            _logger.LogWarning("[SceneRepository -> GetEndingSceneByTypeAsync] Invalid storyId provided: {storyId}", storyId);
            return null;
        }

        try
        {
            var ending = await _db.EndingScenes
               .AsNoTracking()
               .FirstOrDefaultAsync(e => e.StoryId == storyId && e.EndingType == type);

            if (ending == null)
            {
                _logger.LogInformation("[SceneRepository -> GetEndingSceneByTypeAsync] No {typeName} EndingScene found for storyId {storyId}", typeName, storyId);
            }

            return ending;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[SceneRepository -> GetEndingSceneByTypeAsync] Error retrieving {typeName} EndingScene for storyId {storyId}", typeName, storyId);
            return null;
        }
    }

    public async Task<bool> AddEndingScene(EndingScene endingScene) // not in use
    {
        if (endingScene == null)
        {
            _logger.LogWarning("[SceneRepository -> AddEndingScene] Cannot add a null EndingScene");
            return false;
        }

        try
        {
            _db.EndingScenes.Add(endingScene);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[SceneRepository -> AddEndingScene] Error adding EndingScene {endingScene}", endingScene);
            return false;
        }
    }

    public async Task<bool> UpdateEndingScene(EndingScene endingScene) // not in use
    {
        if (endingScene == null)
        {
            _logger.LogWarning("[SceneRepository -> UpdateEndingScene] Cannot update a null EndingScene");
            return false;
        }

        try
        {
            _db.EndingScenes.Update(endingScene);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[SceneRepository -> UpdateEndingScene] Error updating EndingScene {endingScene}", endingScene);
            return false;
        }
    }

    public async Task<bool> UpdateEndingScenes(IEnumerable<EndingScene> endingScenes)
    {
        var endingScenesList = endingScenes?.ToList() ?? new List<EndingScene>();
        if (!endingScenesList.Any())
        {
            _logger.LogWarning("[SceneRepository -> UpdateEndingScenes] No EndingScenes provided");
            return false;
        }

        try
        {
            var ids = endingScenesList.Select(e => e.EndingSceneId).ToList();
            var existingScenes = await _db.EndingScenes
                .Where(e => ids.Contains(e.EndingSceneId))
                .ToListAsync();

            foreach (var updated in endingScenesList)
            {
                var existing = existingScenes.FirstOrDefault(e => e.EndingSceneId == updated.EndingSceneId);
                if (existing == null)
                {
                    _logger.LogWarning("[SceneRepository -> UpdateEndingScenes] EndingScene {EndingSceneId} not found", updated.EndingSceneId);
                    continue;
                }

                existing.EndingText = updated.EndingText;
            }

            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[SceneRepository -> EditEndingScenes] Error updating one or more EndingScenes");
            return false;
        }
    }

    public async Task<bool> DeleteEndingScene(int endingSceneId) // not in use
    {
        if (endingSceneId <= 0)
        {
            _logger.LogWarning("[SceneRepository -> DeleteEndingScene] Invalid EndingSceneId provided: {endingSceneId}", endingSceneId);
            return false;
        }

        try
        {
            var ending = await _db.EndingScenes.FindAsync(endingSceneId);
            if (ending == null)
            {
                _logger.LogWarning("[SceneRepository -> DeleteEndingScene] No EndingScene found with id {endingSceneId}", endingSceneId);
                return false;
            }

            _db.EndingScenes.Remove(ending);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[SceneRepository -> DeleteEndingScene] Error deleting EndingScene with id {endingSceneId}", endingSceneId);
            return false;
        }
    }
}