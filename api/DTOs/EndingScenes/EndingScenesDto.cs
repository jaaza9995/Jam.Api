namespace Jam.Api.DTOs.EndingScenes;

public class EndingSceneDto
{
    public int StoryId { get; set; }
    public string GoodEnding { get; set; } = string.Empty;
    public string NeutralEnding { get; set; } = string.Empty;
    public string BadEnding { get; set; } = string.Empty;
}
