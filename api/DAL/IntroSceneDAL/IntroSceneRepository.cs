using Jam.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Jam.Api.DAL.IntroSceneDAL;

public class IntroSceneRepository : IIntroSceneRepository
{
    private readonly StoryDbContext _db;
    private readonly ILogger<IntroSceneRepository> _logger;

    public IntroSceneRepository(
        StoryDbContext db,
        ILogger<IntroSceneRepository> logger
    )
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IntroScene?> GetIntroSceneByStoryId(int storyId)
    {
        if (storyId <= 0)

        {
            _logger.LogWarning("[SceneRepository -> GetIntroSceneByStoryId] Invalid storyId provided: {storyId}", storyId);
            return null;
        }

        try
        {
            var intro = await _db.IntroScenes
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.StoryId == storyId);

            if (intro == null)
            {
                _logger.LogInformation("[SceneRepository -> GetIntroSceneByStoryId] No IntroScene found for storyId {storyId}", storyId);
            }

            return intro;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[SceneRepository -> GetIntroSceneByStoryId] Error retrieving IntroScene for storyId {storyId}", storyId);
            return null;
        }
    }

    public async Task<IntroScene?> GetIntroSceneById(int introSceneId)
    {
        if (introSceneId <= 0)
        {
            _logger.LogWarning("[SceneRepository -> GetIntroSceneById] Invalid IntroSceneId provided: {introSceneId}", introSceneId);
            return null;
        }

        try
        {
            var intro = await _db.IntroScenes
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.IntroSceneId == introSceneId);

            if (intro == null)
            {
                _logger.LogInformation("[SceneRepository -> GetIntroSceneById] No IntroScene found with id {introSceneId}", introSceneId);
            }

            return intro;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[SceneRepository -> GetIntroSceneById] Error retrieving IntroScene with id {introSceneId}", introSceneId);
            return null;
        }
    }

    public async Task<bool> AddIntroScene(IntroScene introScene) // not in use
    {
        if (introScene == null)
        {
            _logger.LogWarning("[SceneRepository -> AddIntroScene] Cannot add a null IntroScene");
            return false;
        }

        try
        {
            _db.IntroScenes.Add(introScene);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[SceneRepository -> AddIntroScene] Error adding IntroScene {introScene}", introScene);
            return false;
        }
    }

    public async Task<bool> UpdateIntroScene(IntroScene introScene)
    {
        if (introScene == null)
        {
            _logger.LogWarning("[SceneRepository -> UpdateIntroScene] Cannot update a null IntroScene");
            return false;
        }

        try
        {
            _db.IntroScenes.Update(introScene);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[SceneRepository -> UpdateIntroScene] Error updating IntroScene {introScene}", introScene);
            return false;
        }
    }

    public async Task<bool> DeleteIntroScene(int introSceneId) // not in use
    {
        if (introSceneId <= 0)
        {
            _logger.LogWarning("[SceneRepository -> DeleteIntroScene] Invalid IntroScene id provided: {introSceneId}", introSceneId);
            return false;
        }

        try
        {
            var intro = await _db.IntroScenes.FindAsync(introSceneId);
            if (intro == null)
            {
                _logger.LogInformation("[SceneRepository -> DeleteIntroScene] No IntroScene found with id {introSceneId}", introSceneId);
                return false;
            }

            _db.IntroScenes.Remove(intro);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[SceneRepository -> DeleteIntroScene] Error deleting IntroScene with id {introSceneId}", introSceneId);
            return false;
        }
    }
}