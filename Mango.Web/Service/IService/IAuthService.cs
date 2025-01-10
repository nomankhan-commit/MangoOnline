using Mango.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Web.Service.IService
{
    public interface IAuthService
    {
        Task<ResponseDto> LoginAsync(LoginRequestDto loginRequestDto);
        Task<ResponseDto> RegisterAsync(RegistrationRequestDto registrationRequestDto);
        Task<ResponseDto> AssingRoleAsync(RegistrationRequestDto registrationRequestDto);
    }
}
