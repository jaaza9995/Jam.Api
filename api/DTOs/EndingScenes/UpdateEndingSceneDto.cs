using System.ComponentModel.DataAnnotations;

namespace Jam.DTOs.UpdateEndingScenes //put dto
{
    public class UpdateEndingSceneDto
    {
        public int StoryId { get; set; }

        [Required]
        [MinLength(5)]
        public string GoodEnding { get; set; } = string.Empty;

        [Required]
        [MinLength(5)]
        public string NeutralEnding { get; set; } = string.Empty;

        [Required]
        [MinLength(5)]
        public string BadEnding { get; set; } = string.Empty;

    }
}
