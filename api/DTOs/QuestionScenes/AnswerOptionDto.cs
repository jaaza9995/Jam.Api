using System.ComponentModel.DataAnnotations;

public class AnswerOptionDto
{
    public int AnswerOptionId { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "Answer Text must be at least 1 character")]
    public string AnswerText { get; set; } = "";

    [Required]
    [MinLength(1,  ErrorMessage = "Context Text must be at least 1 character")]
    public string ContextText { get; set; } = "";
}
