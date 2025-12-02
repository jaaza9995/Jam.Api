namespace Jam.Api.DTOs.QuestionScenes;

public class QuestionSceneDto
{
    public int QuestionSceneId { get; set; }
    public string StoryText { get; set; } = "";
    public string QuestionText { get; set; } = "";
    public int CorrectAnswerIndex { get; set; }
    public List<AnswerOptionDto> Answers { get; set; } = new();
}
