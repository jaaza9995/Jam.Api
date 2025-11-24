using System.ComponentModel.DataAnnotations;

namespace Jam.Api.DTOs.QuestionScenes;

public class UpdateQuestionSceneDto //put dto 
{
    [Required]
    public int QuestionSceneId { get; set; }

    [Required]
    [MinLength(5, ErrorMessage = "Story Text must be at least 5 characters")]
    public string StoryText { get; set; } = string.Empty;

    [Required]
    [MinLength(5, ErrorMessage = "Question Text must be at least 5 characters")]
    public string QuestionText { get; set; } = string.Empty;

    [Range(0, 3, ErrorMessage = "You must select a correct answer")]
    public int CorrectAnswerIndex { get; set; }

    [MinLength(4)]
    [MaxLength(4)]
    public List<AnswerOptionDto> Answers { get; set; } = new();
    public bool MarkedForDeletion { get; set; }
}
