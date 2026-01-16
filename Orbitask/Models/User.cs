using Microsoft.AspNetCore.Identity;
using System.Data.Common;

namespace Orbitask.Models
{
    public class User : IdentityUser
    {
        public string DisplayName { get; set; } = null!;
        public string? AvatarUrl { get; set; }
    }
}
