using System.ComponentModel.DataAnnotations;

namespace Jam.DTOs.JoinPrivateStoryRequestDto
{
    public class JoinPrivateStoryRequestDto
    {
        [Required]
        [MinLength(5)]
        public string Code { get; set; } = string.Empty;

    }
}
