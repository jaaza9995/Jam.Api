using Jam.Api.Models;

namespace Jam.Api.DAL.IntroSceneDAL;

/// <summary>
/// Methods: AddIntroScene() and DeleteIntroScene() are not in use.
/// Retained for CRUD completeness and future standalone operations.
/// </summary>
public interface IIntroSceneRepository
{
    // --------------- Get / Read ---------------
    Task<IntroScene?> GetIntroSceneByStoryId(int storyId);
    Task<IntroScene?> GetIntroSceneById(int introSceneId);


    // ----------------- Create -----------------
    Task<bool> AddIntroScene(IntroScene introScene);


    // ----------------- Update -----------------
    Task<bool> UpdateIntroScene(IntroScene introScene);


    // ----------------- Delete -----------------
    Task<bool> DeleteIntroScene(int introSceneId);
}