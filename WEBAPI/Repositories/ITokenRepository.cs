using Microsoft.AspNetCore.Identity;

namespace WebAPI.Repositories
{
    public interface ITokenRepository
    {
        string CreateJWTToken(IdentityUser user, List<string> roles);
    }
}