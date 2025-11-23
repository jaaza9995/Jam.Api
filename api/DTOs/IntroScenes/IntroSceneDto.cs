using System.ComponentModel.DataAnnotations;

namespace Jam.DTOs.IntroScenes
{
    public class IntroSceneDto //get dto
    {
        public int StoryId { get; set; }
        [Required]
        [MinLength(10)]
        public string IntroText { get; set; } = string.Empty;
    }
}
