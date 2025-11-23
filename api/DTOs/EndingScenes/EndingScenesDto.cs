namespace Jam.DTOs.EndingScenesDto
{
    public class EndingSceneDto//get dto
    {
        public int StoryId { get; set; }
        public string GoodEnding { get; set; } = string.Empty;
        public string NeutralEnding { get; set; } = string.Empty;
        public string BadEnding { get; set; } = string.Empty;
    }
}
