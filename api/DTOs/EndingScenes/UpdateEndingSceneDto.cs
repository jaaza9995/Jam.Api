using System.ComponentModel.DataAnnotations;

namespace Jam.DTOs.UpdateEndingScenes //put dto
{
    public class UpdateEndingSceneDto
    {
        public int StoryId { get; set; }

        [Required]
        public string GoodEnding { get; set; } = string.Empty;

        [Required]
        public string NeutralEnding { get; set; } = string.Empty;

        [Required]
        public string BadEnding { get; set; } = string.Empty;
    }
}
