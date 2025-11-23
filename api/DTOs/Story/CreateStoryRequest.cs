using System.ComponentModel.DataAnnotations;
using Jam.Models.Enums;

namespace Jam.DTOs.Story
{
    public class CreateStoryRequestDto
    {
        [Required]
        [MinLength(3)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MinLength(10)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DifficultyLevel DifficultyLevel { get; set; }

        [Required]
        public Accessibility Accessibility { get; set; }

        [Required]
        [MinLength(10)]
        public string IntroText { get; set; } = string.Empty;
    }
}
