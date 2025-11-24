using System.ComponentModel.DataAnnotations;

namespace Jam.Api.DTOs.QuestionScenes;

public class QuestionScenesPayload

{
    [Required]
    [MinLength(1, ErrorMessage = "At least one question is required")]
    public List<UpdateQuestionSceneDto> QuestionScenes { get; set; } = new();
}
