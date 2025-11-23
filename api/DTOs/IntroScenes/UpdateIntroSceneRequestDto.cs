using System.ComponentModel.DataAnnotations;

namespace Jam.DTOs.IntroScenes
{
    public class UpdateIntroSceneRequestDto
    {
        [Required]
        public string IntroText { get; set; } = string.Empty;
    }
}
