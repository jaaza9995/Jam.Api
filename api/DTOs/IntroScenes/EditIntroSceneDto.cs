using System.ComponentModel.DataAnnotations;

namespace Jam.DTOs.StoryEditing; //put dto 

public class EditIntroSceneDto
{
    public int StoryId { get; set; }

    [Required]
    [MinLength(10, ErrorMessage = "Intro Text must be at least 10 characters")]
    public string IntroText { get; set; } = string.Empty;
}