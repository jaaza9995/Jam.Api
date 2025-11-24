using System.ComponentModel.DataAnnotations;

namespace Jam.Api.DTOs.IntroScenes;

public class IntroSceneDto //get dto
{
    public int StoryId { get; set; }
    [Required]
    [MinLength(10, ErrorMessage = "Intro Text must be at least 10 characters")]
    public string IntroText { get; set; } = string.Empty;
}
