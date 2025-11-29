using Jam.Api.Models;
using Jam.Api.Models.Enums;
using Jam.Api.DTOs.Story;

namespace Jam.Api.Services;

public class StoryCreationService : IStoryCreationService
{
    private readonly IStoryCodeService _codeService;
    private readonly ILogger<StoryCreationService> _logger;

    public StoryCreationService(IStoryCodeService codeService, ILogger<StoryCreationService> logger)
    {
        _codeService = codeService;
        _logger = logger;
    }

    public async Task<Story?> CreateStoryFromSessionAsync(StoryCreationSession session, AuthUser user)
    {
        if (session == null)
        {
            _logger.LogWarning("[StoryCreationService] Session is null");
            return null;
        }

        if (user == null)
        {
            _logger.LogWarning("[StoryCreationService] User is null");
            return null;
        }

        try
        {
            // Map QuestionScenes from DTO 
            var questionScenes = session.QuestionScenes.Select(q => new QuestionScene
            {
                SceneText = q.StoryText,
                Question = q.QuestionText,
                AnswerOptions = q.Answers.Select((a, i) => new AnswerOption
                {
                    Answer = a.AnswerText,
                    FeedbackText = a.ContextText,
                    IsCorrect = i == q.CorrectAnswerIndex
                }).ToList()
            }).ToList();

            // Link QuestionScenes sequentially
            for (int i = 0; i < questionScenes.Count; i++)
            {
                questionScenes[i].NextQuestionScene = i < questionScenes.Count - 1
                    ? questionScenes[i + 1]
                    : null;
            }

            // Build Story
            var story = new Story
            {
                Title = session.Title,
                Description = session.Description,
                DifficultyLevel = session.DifficultyLevel,
                Accessibility = session.Accessibility,
                UserId = user.Id,
                IntroScene = new IntroScene { IntroText = session.IntroText },
                QuestionScenes = questionScenes,
                EndingScenes = new List<EndingScene>
                {
                    new EndingScene { EndingType = EndingType.Good, EndingText = session.GoodEnding },
                    new EndingScene { EndingType = EndingType.Neutral, EndingText = session.NeutralEnding },
                    new EndingScene { EndingType = EndingType.Bad, EndingText = session.BadEnding }
                }
            };

            // Generate code if private
            if (story.Accessibility == Accessibility.Private)
            {
                story.Code = await _codeService.GenerateUniqueStoryCodeAsync();
            }

            _logger.LogInformation("[StoryCreationService] Story created from session for user {UserId}", user.Id);

            return story;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[StoryCreationService] Error creating story from session");
            return null;
        }
    }

}