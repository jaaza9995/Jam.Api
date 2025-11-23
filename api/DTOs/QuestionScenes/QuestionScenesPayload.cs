using System.ComponentModel.DataAnnotations;

namespace Jam.DTOs.QuestionScenes;
public class QuestionScenesPayload

{
    [Required]
    [MinLength(1)]
    public List<UpdateQuestionSceneDto> QuestionScenes { get; set; } = new();
}
