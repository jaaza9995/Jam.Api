using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jam.Api.Models
{
    public class AuthUser : IdentityUser
    {
        [NotMapped]
        public List<Story> Stories { get; set; } = new();

        [NotMapped]
        public List<PlayingSession> PlayingSessions { get; set; } = new();
    }
}