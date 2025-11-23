using System.ComponentModel.DataAnnotations;
using Jam.Models.Enums;

namespace Jam.DTOs.Story
{
    public class CreateStoryRequestDto
    {
        [Required]
        [MinLength(3, ErrorMessage = "Title must be at least 3 characters")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MinLength(10, ErrorMessage = "Description must be at least 10 characters")]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DifficultyLevel DifficultyLevel { get; set; }

        [Required]
        public Accessibility Accessibility { get; set; }

        [Required]
        [MinLength(10, ErrorMessage = "Intro Text must be at least 10 characters")]
        public string IntroText { get; set; } = string.Empty;
    }
}
