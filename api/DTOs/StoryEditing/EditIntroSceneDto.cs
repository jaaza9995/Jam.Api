using System.ComponentModel.DataAnnotations;

namespace Jam.DTOs.StoryEditing;

public class EditIntroSceneDto
{
    public int StoryId { get; set; }

    [Required]
    public string IntroText { get; set; } = string.Empty;
}