using System.ComponentModel.DataAnnotations;

namespace Jam.DTOs.StoryCreation;

public class CreateMultipleQuestionScenesDto
{
    // public int StoryId { get; set; } no longer need this
    // public int? PreviousSceneId { get; set; } no longer need this

    [MinLength(1, ErrorMessage = "Please add at least one question scene.")]
    public List<QuestionSceneBaseDto> QuestionScenes { get; set; } = new();
}