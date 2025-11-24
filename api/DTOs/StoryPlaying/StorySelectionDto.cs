using Jam.Models;

namespace Jam.Api.DTOs.StoryPlaying;

public class StorySelectionDto
{
    public IEnumerable<Story> PublicStories { get; set; } = new List<Story>();
    public IEnumerable<Story> PrivateStories { get; set; } = new List<Story>();
}