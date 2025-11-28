namespace Jam.Api.Services;

public interface IStoryCodeService
{
    Task<string> GenerateUniqueStoryCodeAsync();
}