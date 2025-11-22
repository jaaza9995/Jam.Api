using System.ComponentModel.DataAnnotations;

namespace Jam.DTOs.StoryCreation;

public class CreateMultipleQuestionScenesDto
{
    // public int StoryId { get; set; } no longer need this
    // public int? PreviousSceneId { get; set; } no longer need this

   [Required]
    public List<QuestionSceneBaseDto> QuestionScenes { get; set; } = new();
}