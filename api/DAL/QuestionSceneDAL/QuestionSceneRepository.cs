using Jam.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Jam.Api.DAL.QuestionSceneDAL;

public class QuestionSceneRepository : IQuestionSceneRepository
{
    private readonly StoryDbContext _db;
    private readonly ILogger<QuestionSceneRepository> _logger;

    public QuestionSceneRepository(
        StoryDbContext db,
        ILogger<QuestionSceneRepository> logger
    )
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IEnumerable<QuestionScene>> GetQuestionScenesByStoryId(int storyId)
    {
        if (storyId <= 0)
        {
            _logger.LogWarning("[SceneRepository -> GetQuestionScenesByStoryId] Invalid storyId provided: {storyId}", storyId);
            return Enumerable.Empty<QuestionScene>(); // not returning null to avoid null reference exceptions
        }

        try
        {
            // Step 1: Eagerly load ALL QuestionScenes and their AnswerOptions for the story
            var allQuestionScenes = await _db.QuestionScenes
                .Where(q => q.StoryId == storyId)
                .Include(q => q.AnswerOptions) // eager load AnswerOptions (not sure if wanted here)
                .ToListAsync();

            if (!allQuestionScenes.Any())
            {
                _logger.LogWarning("[SceneRepository -> GetQuestionScenesByStoryId] No QuestionScenes found for story with id {storyId}", storyId);
                return Enumerable.Empty<QuestionScene>();
            }

            // Step 2: Find the starting scene using the existing logic 
            var firstScene = await GetFirstQuestionSceneByStoryId(storyId);

            if (firstScene == null)
            {
                _logger.LogWarning("[SceneRepository -> GetQuestionScenesByStoryId] Could not find starting scene for storyId {storyId}. Returning list sorted by QuestionSceneId", storyId);
                return allQuestionScenes.OrderBy(s => s.QuestionSceneId); // fallback to return the list sorted by QuestionSceneId (not ideal, but better than nothing)
            }

            // Step 3: Chain the scenes in memory
            return ChainScenes(allQuestionScenes, firstScene.QuestionSceneId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[SceneRepository -> GetQuestionScenesByStoryId] Error retrieving QuestionScenes for storyId {storyId}", storyId);
            return Enumerable.Empty<QuestionScene>(); ; // not returning null to avoid null reference exceptions
        }
    }

    // Sorts the list of scenes into the correct order based on the NextQuestionSceneId chain.
    // <param name="allScenes">All QuestionScene objects belonging to the story.</param>
    // <param name="startingSceneId">The ID of the first scene in the sequence.</param>
    // <returns>An IEnumerable of QuestionScene objects in the correct story order.</returns>
    private IEnumerable<QuestionScene> ChainScenes(List<QuestionScene> allQuestionScenes, int startingSceneId)
    {
        if (allQuestionScenes == null || !allQuestionScenes.Any())
        {
            return Enumerable.Empty<QuestionScene>();
        }

        // Convert the list to a Dictionary for O(1) lookup speed (by ID)
        var questionSceneMap = allQuestionScenes.ToDictionary(s => s.QuestionSceneId);

        var orderedScenes = new List<QuestionScene>();
        int? currentId = startingSceneId;

        // Walk the chain until the current ID is null or we hit a scene not in the map
        while (currentId.HasValue && questionSceneMap.TryGetValue(currentId.Value, out var currentScene))
        {
            orderedScenes.Add(currentScene);
            currentId = currentScene.NextQuestionSceneId;

            // Safety break to prevent infinite loop if the chain is circular
            if (orderedScenes.Count > allQuestionScenes.Count)
            {
                _logger.LogWarning("[SceneRepository -> ChainScenes] Infinite loop detected in scene chain for scene {id}.", startingSceneId);
                break;
            }
        }

        return orderedScenes;
    }

    public async Task<QuestionScene?> GetFirstQuestionSceneByStoryId(int storyId)
    {
        if (storyId <= 0)
        {
            _logger.LogWarning("[SceneRepository -> GetFirstQuestionSceneByStoryId] Invalid storyId provided: {storyId}", storyId);
            return null;
        }

        try
        {
            // Step 1: Collect all NextQuestionSceneIds for the given story
            var nonStartingIds = await _db.QuestionScenes
                .Where(qs => qs.StoryId == storyId && qs.NextQuestionSceneId.HasValue)
                .Select(qs => qs.NextQuestionSceneId!.Value)
                .Distinct()
                .ToListAsync();

            // Step 2: Find the single QuestionScene that belongs to the story 
            // AND whose ID is NOT in the list of non-starting IDs collected above
            var firstQuestionScene = await _db.QuestionScenes
                .Where(qs => qs.StoryId == storyId && !nonStartingIds.Contains(qs.QuestionSceneId))
                .FirstOrDefaultAsync();

            return firstQuestionScene;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[SceneRepository -> GetFirstQuestionSceneByStoryId] Error retrieving first QuestionScene for storyId {storyId}", storyId);
            return null;
        }
    }

    public async Task<QuestionScene?> GetQuestionSceneById(int questionSceneId) // not in use
    {
        if (questionSceneId <= 0)
        {
            _logger.LogWarning("[SceneRepository -> GetQuestionSceneById] Invalid QuestionSceneId provided: {questionSceneId}", questionSceneId);
            return null;
        }

        try
        {
            var questionScene = await _db.QuestionScenes
               .AsNoTracking()
               .FirstOrDefaultAsync(q => q.QuestionSceneId == questionSceneId);

            if (questionScene == null)
            {
                _logger.LogWarning("[SceneRepository -> GetQuestionSceneById] No QuestionScene found with id {questionSceneId}", questionSceneId);
            }

            return questionScene;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[SceneRepository -> GetQuestionSceneById] Error retrieving QuestionScene with id {questionSceneId}", questionSceneId);
            return null;
        }
    }

    public async Task<QuestionScene?> GetNextQuestionSceneById(int currentQuestionSceneId)
    {
        if (currentQuestionSceneId <= 0)
        {
            _logger.LogWarning("[SceneRepository -> GetQuestionSceneById] Invalid QuestionSceneId provided: {currentQuestionSceneId}", currentQuestionSceneId);
            return null;
        }

        try
        {
            var currentScene = await _db.QuestionScenes
                .AsNoTracking()
                .Include(q => q.NextQuestionScene)
                .FirstOrDefaultAsync(q => q.QuestionSceneId == currentQuestionSceneId);

            if (currentScene == null)
            {
                _logger.LogWarning("[SceneRepository -> GetNextQuestionScene] Current QuestionScene with id {currentQuestionSceneId} not found.", currentQuestionSceneId);
                return null;
            }

            return currentScene.NextQuestionScene;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[SceneRepository -> GetNextQuestionSceneById] Error retrieving next QuestionScene with id {currentQuestionSceneId}", currentQuestionSceneId);
            return null;
        }
    }

    // New method to get QuestionScene with its AnswerOptions eagerly loaded
    public async Task<QuestionScene?> GetQuestionSceneWithAnswerOptionsById(int questionSceneId)
    {
        if (questionSceneId <= 0)
        {
            _logger.LogWarning("[SceneRepository -> GetQuestionSceneWithAnswerOptionsById] Invalid QuestionSceneId provided: {questionSceneId}", questionSceneId);
            return null;
        }

        try
        {
            var questionScene = await _db.QuestionScenes
                .Include(q => q.AnswerOptions)
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.QuestionSceneId == questionSceneId);

            if (questionScene == null)
            {
                _logger.LogInformation("[SceneRepository -> GetQuestionSceneWithAnswerOptionsById] No QuestionScene found with id {questionSceneId}", questionSceneId);
            }

            return questionScene;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[SceneRepository -> GetQuestionSceneWithAnswerOptionsById] Error retrieving QuestionScene with id {questionSceneId}", questionSceneId);
            return null;
        }
    }


    public async Task<bool> AddQuestionScene(QuestionScene questionScene)
    {
        if (questionScene == null)
        {
            _logger.LogWarning("[SceneRepository -> AddQuestionScene] Cannot add a null QuestionScene");
            return false;
        }

        try
        {
            _db.QuestionScenes.Add(questionScene);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[SceneRepository -> AddQuestionScene] Error adding QuestionScene for storyId {storyId}", questionScene.StoryId);
            return false;
        }
    }

    public async Task<bool> UpdateQuestionScene(QuestionScene questionScene)
    {
        if (questionScene == null)
        {
            _logger.LogWarning("[SceneRepository -> UpdateQuestionScene] Cannot update a null QuestionScene");
            return false;
        }

        try
        {
            _db.QuestionScenes.Update(questionScene);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[SceneRepository -> UpdateQuestionScene] Error updating QuestionScene with id {questionSceneId}", questionScene.QuestionSceneId);
            return false;
        }
    }

    public async Task<bool> DeleteQuestionScene(int questionSceneId)
    {
        if (questionSceneId <= 0)
        {
            _logger.LogWarning("[SceneRepository -> DeleteQuestionScene] Invalid QuestionSceneId {questionSceneId}", questionSceneId);
            return false;
        }

        try
        {
            var scene = await _db.QuestionScenes.FindAsync(questionSceneId);
            if (scene == null)
            {
                _logger.LogWarning("[SceneRepository -> DeleteQuestionScene] Scene not found with id {questionSceneId}", questionSceneId);
                return false;
            }

            _db.QuestionScenes.Remove(scene);
            await _db.SaveChangesAsync();

            _logger.LogInformation("[SceneRepository -> DeleteQuestionScene] Deleted scene {Id}", questionSceneId);

            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[SceneRepository -> DeleteQuestionScene] Error deleting scene {Id}", questionSceneId);
            return false;
        }
    }
}