namespace Jam.Api.Services;

public interface ITokenService
{
    Task<string> GenerateJwtTokenAsync(Models.AuthUser user);
}