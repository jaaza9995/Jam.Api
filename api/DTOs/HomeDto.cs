using Jam.Models;

namespace Jam.DTOs;

public class HomeDto
{
    public string FirstName { get; set; } = string.Empty;
    public IEnumerable<Story> YourGames { get; set; } = new List<Story>();
    public IEnumerable<Story> RecentlyPlayed { get; set; } = new List<Story>();
}