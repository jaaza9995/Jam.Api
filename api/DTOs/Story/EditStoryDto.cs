using System.ComponentModel.DataAnnotations; //keep
using Jam.Models.Enums;

namespace Jam.DTOs.StoryEditing;

public class EditStoryDto
{
    public int StoryId { get; set; }

    [Required]
    [MinLength(3)]
    public string Title { get; set; } = string.Empty;


    [Required]
    [MinLength(10)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DifficultyLevel DifficultyLevel { get; set; }

    [Required]
    public Accessibility Accessibility { get; set; }

    public string? Code { get; set; }
}