public class StorySelectionDto
{
    public IEnumerable<StoryCardDto> PublicStories { get; set; } = new List<StoryCardDto>();
    public IEnumerable<StoryCardDto> PrivateStories { get; set; } = new List<StoryCardDto>();
}
