using Jam.Api.Models;

namespace Jam.Api.DAL.AnswerOptionDAL;

/// <summary>
/// Methods: GetAllAnswerOptions(), GetAnswerOptionsByQuestionSceneId(),
/// AddAnswerOption(), UpdateAnswerOption(), and DeleteAnswerOption() are not in use
/// Retained for CRUD completeness and future standalone operations.
/// </summary>
public interface IAnswerOptionRepository
{
    // Read / GET
    Task<IEnumerable<AnswerOption>> GetAllAnswerOptions(); 
    Task<IEnumerable<AnswerOption>> GetAnswerOptionsByQuestionSceneId(int questionSceneId); 
    Task<AnswerOption?> GetAnswerOptionById(int answerOptionId);


    // Create
    Task<bool> AddAnswerOption(AnswerOption answerOption);


    // Update
    Task<bool> UpdateAnswerOption(AnswerOption answerOption);


    // Delete
    Task<bool> DeleteAnswerOption(int answerOptionId);
}