using Jam.Api.DAL.SceneDAL;
using Jam.Api.DTOs.QuestionScenes;
using Jam.Api.Models;
using System.Transactions;

namespace Jam.Api.Services;

/// <summary>
/// StoryEditingService handles the business logic for updating a story's question scenes.
/// 
/// The UpdateQuestionScenesAsync method orchestrates a multi-step update process:
/// 1. Identifies which scenes need to be deleted (present in DB but not in the new list)
/// 2. Adds new scenes and updates existing ones, preserving their order
/// 3. Relinks scenes by updating NextQuestionSceneId to maintain the scene chain
/// 
/// All operations are wrapped in a TransactionScope to ensure atomicity:
/// - If any step fails, the entire transaction rolls back, preventing partial/inconsistent updates
/// - This guarantees that either all changes are saved or none are saved
/// - Critical because breaking the scene chain or leaving orphaned data would corrupt the story
/// </summary>
public class StoryEditingService : IStoryEditingService
{
    private readonly ISceneRepository _sceneRepository;
    private readonly ILogger<StoryPlayingService> _logger;

    public StoryEditingService(
        ISceneRepository sceneRepository,
        ILogger<StoryPlayingService> logger
    )
    {
        _sceneRepository = sceneRepository;
        _logger = logger;
    }

    public async Task<bool> UpdateQuestionScenesAsync(int storyId, List<UpdateQuestionSceneDto> newScenes)
    {
        if (newScenes == null)
        {
            _logger.LogError("[StoryEditingService -> UpdateQuestionScenesAsync] newScenes is null for Story {StoryId}.", storyId);
            return false;
        }

        using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                // 1. Retrieve existing QuestionScenes (to find the ones to delete)
                var existingScenes = await _sceneRepository.GetQuestionScenesByStoryId(storyId);

                // 2. Find QuestionScenes to delete 
                var newSceneIds = newScenes
                    .Where(q => q.QuestionSceneId != 0)
                    .Select(q => q.QuestionSceneId)
                    .ToHashSet();
                var toDelete = existingScenes
                    .Where(s => !newSceneIds
                    .Contains(s.QuestionSceneId))
                    .ToList();

                int addedCount = 0;
                int updatedCount = 0;
                int deletedCount = 0;

                // 3. Delete QuestionScenes
                foreach (var scene in toDelete)
                {
                    await _sceneRepository.DeleteQuestionScene(scene.QuestionSceneId);
                    deletedCount++;
                }

                // 4. Loop to update, add, and track order
                var trackedInOrder = new List<QuestionScene>();

                foreach (var newScene in newScenes)
                {
                    QuestionScene? sceneToUpdate;

                    if (newScene.QuestionSceneId == 0)
                    {
                        // New QuestionScene (add to db)
                        sceneToUpdate = new QuestionScene
                        {
                            SceneText = newScene.StoryText, // should be Scenetext
                            Question = newScene.QuestionText,
                            StoryId = storyId,
                            AnswerOptions = newScene.Answers.Select(a => new AnswerOption
                            {
                                // Map properties from DTO to AnswerOption
                                Answer = a.AnswerText,
                                FeedbackText = a.ContextText,
                                IsCorrect = newScene.Answers.IndexOf(a) == newScene.CorrectAnswerIndex
                            }).ToList()
                        };

                        await _sceneRepository.AddQuestionScene(sceneToUpdate);
                        addedCount++;
                    }
                    else
                    {
                        // Existing QuestionScene (update in db)
                        sceneToUpdate = existingScenes.FirstOrDefault(q => q.QuestionSceneId == newScene.QuestionSceneId);
                        if (sceneToUpdate == null) continue;

                        sceneToUpdate.SceneText = newScene.StoryText;
                        sceneToUpdate.Question = newScene.QuestionText;

                        // Update AnswerOptions
                        sceneToUpdate.AnswerOptions.Clear();
                        foreach (var answer in newScene.Answers)
                        {
                            sceneToUpdate.AnswerOptions.Add(new AnswerOption
                            {
                                // Map properties from DTO to AnswerOption
                                Answer = answer.AnswerText,
                                FeedbackText = answer.ContextText,
                                IsCorrect = newScene.Answers.IndexOf(answer) == newScene.CorrectAnswerIndex
                            });
                        }

                        await _sceneRepository.UpdateQuestionScene(sceneToUpdate);
                        updatedCount++;
                    }

                    trackedInOrder.Add(sceneToUpdate);
                }

                // 5. Update NextQuestionSceneId to link in saved order
                for (int i = 0; i < trackedInOrder.Count; i++)
                {
                    trackedInOrder[i].NextQuestionSceneId = (i < trackedInOrder.Count - 1) ? trackedInOrder[i + 1].QuestionSceneId : (int?)null;
                    await _sceneRepository.UpdateQuestionScene(trackedInOrder[i]);
                }

                // Complete transaction
                transaction.Complete();

                // Log result
                _logger.LogInformation(
                    "[StoryEditingService -> UpdateQuestionScenesAsync] Changes saved for storyId={storyId}. " +
                    "Added={addedCount}, Updated={updatedCount}, Deleted={deletedCount}",
                    storyId, addedCount, updatedCount, deletedCount
                );

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[StoryEditingService -> UpdateQuestionScenesAsync] Error updating scenes for storyId={storyId}. Transaction rolled back.", storyId);
                transaction.Dispose(); // Rollback happens automatically
                return false;
            }
        }
    }
}