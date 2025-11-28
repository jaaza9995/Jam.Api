using Jam.Api.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Jam.Api.DTOs.Story;

public class EditStoryDto
{
    public int StoryId { get; set; }

    [Required]
    [MinLength(3, ErrorMessage = "Title must be at least 10 characters")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MinLength(10, ErrorMessage = "Description must be at least 10 characters")]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DifficultyLevel DifficultyLevel { get; set; }

    [Required]
    public Accessibility Accessibility { get; set; }

    public string? Code { get; set; }

    public int QuestionCount { get; set; }
}