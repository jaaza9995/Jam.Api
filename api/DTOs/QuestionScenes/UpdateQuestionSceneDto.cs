using System.ComponentModel.DataAnnotations;

namespace Jam.DTOs.QuestionScenes;

public class UpdateQuestionSceneDto //put dto 
{
    [Required]
    public int QuestionSceneId { get; set; }

    [Required]
    [MinLength(5)]
    public string StoryText { get; set; } = string.Empty;

    [Required]
    [MinLength(5)]
    public string QuestionText { get; set; } = string.Empty;

    [Range(0, 3)]
    public int CorrectAnswerIndex { get; set; }

    [MinLength(4)]
    [MaxLength(4)]
    public List<AnswerOptionDto> Answers { get; set; } = new();
    public bool MarkedForDeletion { get; set; }
}
