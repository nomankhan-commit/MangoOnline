using Mango.Services.AuthAPI.Models;

namespace Mango.Services.AuthAPI.Service.IService
{
    public interface IJWTokenGenerator
    {
        //string GenerateToken(ApplicationUser applicationUsers);
        string GenerateToken(ApplicationUser applicationUsers, IEnumerable<string> roles);
    }
}
