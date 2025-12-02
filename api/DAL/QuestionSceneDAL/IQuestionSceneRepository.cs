using Jam.Api.Models;

namespace Jam.Api.DAL.QuestionSceneDAL;

/// <summary>
/// Method: GetQuestionSceneById() is not in use.
/// Retained for CRUD completeness and future standalone operations.
/// </summary>
public interface IQuestionSceneRepository
{
    // ----------------------- Get / Read -----------------------
    Task<IEnumerable<QuestionScene>> GetQuestionScenesByStoryId(int storyId);
    Task<QuestionScene?> GetFirstQuestionSceneByStoryId(int storyId);
    Task<QuestionScene?> GetQuestionSceneById(int questionSceneId); 
    Task<QuestionScene?> GetNextQuestionSceneById(int currentQuestionSceneId); 
    Task<QuestionScene?> GetQuestionSceneWithAnswerOptionsById(int questionSceneId);


    // ----------------------- Create ----------------------- 
    Task<bool> AddQuestionScene(QuestionScene questionScene);


    // ----------------------- Update -----------------------
    Task<bool> UpdateQuestionScene(QuestionScene questionScene); 


    // ----------------------- Delete -----------------------
    Task<bool> DeleteQuestionScene(int questionSceneId);
}