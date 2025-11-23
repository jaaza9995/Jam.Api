using  Jam.Models.Enums;
namespace Jam.DTOs.StoryPlaying

{
    public class StartStoryResponseDto
    {
        public int SessionId { get; set; }
        public int? SceneId { get; set; }
        public SceneType? SceneType { get; set; }
    }
}
