using Microsoft.AspNetCore.Identity;

namespace Jam.Models
{
    public class AuthUser : IdentityUser
    {
        public List<Story> Stories { get; set; } = new();
        public List<PlayingSession> PlayingSessions { get; set; } = new();
    }
}