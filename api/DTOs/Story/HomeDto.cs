using System.Collections.Generic;

namespace Jam.DTOs
{
    public class HomeDto
    {
        public string FirstName { get; set; } = string.Empty;

        public IEnumerable<StoryCardDto> YourGames { get; set; } = new List<StoryCardDto>();
        public IEnumerable<StoryCardDto> RecentlyPlayed { get; set; } = new List<StoryCardDto>();
    }
}
