namespace Jam.DTOs.StoryPlaying;

public class FinishStoryDto()
{
    public string StoryTitle { get; set; } = string.Empty;
    public int FinalScore { get; set; }
    public int MaxScore { get; set; }

    // public DateTime StartTime { get; set; }
    // public DateTime EndTime { get; set; }
    // public TimeSpan Duration { get; set; }
}