using Jam.Api.Models;
using Jam.Api.Models.Enums;

namespace Jam.Api.DAL.PlayingSessionDAL;

/// <summary>
/// Methods: GetAllPlayingSessions(), GetPlayingSessionsByUserId(),
/// GetPlayingSessionsByStoryId(), and DeletePlayingSession() are not in use
/// Retained for CRUD completeness and future standalone operations.
/// </summary>
public interface IPlayingSessionRepository
{
    // Read / GET
    Task<IEnumerable<PlayingSession>> GetAllPlayingSessions();
    Task<PlayingSession?> GetPlayingSessionById(int playingSessionId);
    Task<IEnumerable<PlayingSession>> GetPlayingSessionsByUserId(string userId);
    Task<IEnumerable<PlayingSession>> GetPlayingSessionsByStoryId(int storyId);


    // Create
    Task<bool> AddPlayingSession(PlayingSession playingSession);


    // Update
    Task<bool> TransitionFromIntroToFirstQuestion(int playingSessionId, int nextSceneId, SceneType newSceneType); // new name (old: MoveToNextScene)
    Task<bool> AnswerQuestion(int playingSessionId, int? nextSceneId, SceneType newSceneType, int newScore, int newLevel);
    Task<bool> FinishSession(int playingSessionId, int finalScore, int finalLevel);


    // Delete
    Task<bool> DeletePlayingSession(int playingSessionId);
} 