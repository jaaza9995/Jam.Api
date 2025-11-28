using System.ComponentModel.DataAnnotations;

namespace Jam.Api.DTOs.EndingScenes;

public class UpdateEndingSceneDto
{
    public int StoryId { get; set; }

    [Required]
    [MinLength(5, ErrorMessage = "Good ending must be at least 5 characters")]
    public string GoodEnding { get; set; } = string.Empty;

    [Required]
    [MinLength(5, ErrorMessage = "Neutral ending must be at least 5 characters")]
    public string NeutralEnding { get; set; } = string.Empty;

    [Required]
    [MinLength(5, ErrorMessage = "Bad ending must be at least 5 characters")]
    public string BadEnding { get; set; } = string.Empty;

}

