using System.ComponentModel.DataAnnotations;

namespace Jam.DTOs.StoryEditing;

public class EditIntroSceneDto
{
    public int StoryId { get; set; }

    [Required(ErrorMessage = "Please provide the introduction text for this story.")]
    [StringLength(1500, ErrorMessage = "The introduction text cannot exceed 1000 characters.")]
    [Display(Name = "Introduction")]
    public string IntroText { get; set; } = string.Empty;
}