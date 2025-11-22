namespace Jam.DTOs; // Used by other ViewModels in different sub-directories 

// Used by CreateQuestionSceneViewModel and EditQuestionScenesViewModel
public class QuestionSceneBaseDto
{
    public int StoryId { get; set; }

    public string StoryText { get; set; } = string.Empty;

    public string QuestionText { get; set; } = string.Empty;
    public List<AnswerOptionInput> Answers { get; set; } = new()
    {
        new(), new(), new(), new()
    };
    public int CorrectAnswerIndex { get; set; } = -1;

    public bool IsEditing { get; set; }

    public int QuestionSceneId { get; set; }
    public bool MarkedForDeletion { get; set; } = false;
}

public class AnswerOptionInput
{
    public int AnswerOptionId { get; set; }    
    public string AnswerText { get; set; } = string.Empty;
    public string ContextText { get; set; } = string.Empty;
}
