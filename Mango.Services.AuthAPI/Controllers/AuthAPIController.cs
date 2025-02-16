using AutoMapper;
using Mango.MessageBus;
using Mango.Services.AuthAPI.Models.Dto;
using Mango.Services.AuthAPI.RabbitMQSender;
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
        //private readonly IMessageBus _messageBus;
        private  readonly IRabbitMQAuthMessageSender _messageBus;
        protected ResponseDto _responseDto;
        private IMapper _mapper;
        private IConfiguration _configuration;


        public AuthAPIController(IAuthService authService, IMapper mapper, IConfiguration configuration, IRabbitMQAuthMessageSender messageBus)
        {

            _authService = authService;
            _mapper = mapper;
            _responseDto = new();
            _configuration = configuration;
            _messageBus = messageBus;

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
            _messageBus.SendMessage(model.Email, _configuration.GetValue<string>("TopicsAndQueueName:RegisterUserQueue"));
            return Ok(_responseDto);

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto model)
        {

            var loginresponse = await _authService.Login(model);
            if (loginresponse.User == null)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = "Username or Password is incorrect.";
                return BadRequest(_responseDto);
            }
            _responseDto.Result = loginresponse;
            return Ok(_responseDto);

        }

        [HttpPost("AssingRole")]
        public async Task<IActionResult> AssingRole(RegistrationRequestDto model)
        {

            bool assignRoleSuccessfully = await _authService.AssignRole(model.Email, model.Role.ToUpper());
            if (!assignRoleSuccessfully)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = "Error encountered";
                return BadRequest(_responseDto);
            }
            return Ok(_responseDto);

        }

    }
}
