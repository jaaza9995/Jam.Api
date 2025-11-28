using Jam.Api.DAL.StoryDAL;

namespace Jam.Api.Services;

public class StoryCodeService : IStoryCodeService
{
    private readonly IStoryRepository _storyRepository;

    public StoryCodeService(IStoryRepository storyRepository)
    {
        _storyRepository = storyRepository;
    }

    public async Task<string> GenerateUniqueStoryCodeAsync()
    {
        string code;
        bool exists;

        do
        {
            code = Guid.NewGuid().ToString("N")[..8].ToUpper();
            exists = await _storyRepository.DoesCodeExist(code);
        }
        while (exists);

        return code;
    }
}
