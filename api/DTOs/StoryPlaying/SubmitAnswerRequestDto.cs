using System.ComponentModel.DataAnnotations;
namespace Jam.DTOs.StoryPlaying
{
    public class SubmitAnswerRequestDto
    {
        [Range(1, int.MaxValue)]
        public int SessionId { get; set; }

        [Range(1, int.MaxValue)]
        public int SelectedAnswerId { get; set; }

    }
}
