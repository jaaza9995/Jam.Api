using System.ComponentModel.DataAnnotations;

public class AnswerOptionDto
{
    public int AnswerOptionId { get; set; }

    [Required]
    [MinLength(1)]
    public string AnswerText { get; set; } = "";

    [Required]
    [MinLength(1)]
    public string ContextText { get; set; } = "";
}
