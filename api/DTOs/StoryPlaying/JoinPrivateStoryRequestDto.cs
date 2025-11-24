using System.ComponentModel.DataAnnotations;

namespace Jam.Api.DTOs.StoryPlaying;

public class JoinPrivateStoryRequestDto
{
    [Required]
    [MinLength(5)]
    public string Code { get; set; } = string.Empty;

}
