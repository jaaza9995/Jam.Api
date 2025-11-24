public class StoryCardDto //keep
{
    public int StoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int QuestionCount { get; set; }
    public string Accessibility { get; set; } = string.Empty;
    public string DifficultyLevel{get; set;} = string.Empty;
    public string Code { get; set; } = string.Empty;  // ✔ Viktig!
}