using Jam.Api.DTOs.QuestionScenes;

namespace Jam.Api.Services;

public interface IStoryEditingService
{
    Task<bool> UpdateQuestionScenesAsync(int storyId, List<UpdateQuestionSceneDto> newScenes);
}