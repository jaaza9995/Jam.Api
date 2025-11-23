namespace Jam.DTOs.QuestionScenes;

public class UpdateQuestionSceneDto //put dto 
{
 public int QuestionSceneId { get; set; }
public string StoryText { get; set; } = string.Empty;
public string QuestionText { get; set; } = string.Empty;
public int CorrectAnswerIndex { get; set; }
public List<AnswerOptionDto> Answers { get; set; } = new();
public bool MarkedForDeletion { get; set; }
}
