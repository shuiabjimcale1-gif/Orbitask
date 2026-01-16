using Orbitask.Models;

namespace Orbitask.Services.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }
}