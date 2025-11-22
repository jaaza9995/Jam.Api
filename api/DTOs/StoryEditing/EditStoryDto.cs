using System.ComponentModel.DataAnnotations;
using Jam.Models.Enums;

namespace Jam.DTOs.StoryEditing;

public class EditStoryDto
{
    public int StoryId { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DifficultyLevel DifficultyLevel { get; set; }

    [Required]
    public Accessibility Accessibility { get; set; }

    public string? Code { get; set; }
}