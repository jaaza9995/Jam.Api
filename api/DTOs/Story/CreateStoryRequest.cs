using System.ComponentModel.DataAnnotations;
using Jam.Models.Enums;

namespace Jam.DTOs.Story
{
    public class CreateStoryRequestDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DifficultyLevel DifficultyLevel { get; set; }

        [Required]
        public Accessibility Accessibility { get; set; }

        [Required]
        public string IntroText { get; set; } = string.Empty;
    }
}
