using Jam.Models.Enums;

namespace Jam.Api.DTOs.Admin;

public class StoryDataDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public Accessibility Accessibility { get; set; }
    public string UserId { get; set; } = string.Empty;
}