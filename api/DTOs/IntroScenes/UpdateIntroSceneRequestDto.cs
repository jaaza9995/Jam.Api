using System.ComponentModel.DataAnnotations;

namespace Jam.DTOs.IntroScenes
{
    public class UpdateIntroSceneRequestDto
    {
        [Required]
        [MinLength(10)]
        public string IntroText { get; set; } = string.Empty;

    }
}
