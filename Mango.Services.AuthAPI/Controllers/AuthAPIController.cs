using AutoMapper;
using Mango.Services.AuthAPI.Models.Dto;
using Mango.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.AuthAPI.Controllers
{

//    {
//  "email": "mnk@gmail.com",
//  "name": "Noman Khan",
//  "phoneNumber": "0358999541",
//  "password": "AdminA#44A4"
//}

[Route("api/auth")]
    [ApiController]
    public class AuthAPIController : ControllerBase
    {
        private readonly IAuthService _authService;
        protected ResponseDto _responseDto;
        private IMapper _mapper;
        private IConfiguration _configuration;

        public AuthAPIController(IAuthService authService, IMapper mapper, IConfiguration configuration)
        {

            _authService = authService;
            _mapper = mapper;
            _responseDto = new();
            _configuration = configuration;

        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegistrationRequestDto model)
        {

            string errormsg = await _authService.Register(model);
            if (!string.IsNullOrEmpty(errormsg))
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = errormsg;
                return BadRequest(_responseDto);
            }
            return Ok(_responseDto);

        }
    }
}
