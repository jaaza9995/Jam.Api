using System.ComponentModel.DataAnnotations;

namespace Jam.DTOs.StoryEditing; //put dto 

public class EditIntroSceneDto
{
    public int StoryId { get; set; }

    [Required]
    public string IntroText { get; set; } = string.Empty;
}