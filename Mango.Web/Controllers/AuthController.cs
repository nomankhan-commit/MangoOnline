using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utility;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace Mango.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            LoginRequestDto loginRequestDto = new();
            return View(loginRequestDto);
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            InItRoleDDl();
            RegistrationRequestDto registrationRequest = new();
            return View(registrationRequest);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            RegistrationRequestDto registrationRequest = new();
            return View(registrationRequest);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegistrationRequestDto obj)
        {
            ResponseDto result = await _authService.RegisterAsync(obj);

            if (result != null && result.IsSuccess)
            {
                ResponseDto assignRole;
                if (string.IsNullOrEmpty(obj.Role))
                {
                    obj.Role = SD.RoleCustomer;
                }
                assignRole = await _authService.AssingRoleAsync(obj);
                if (assignRole != null && assignRole.IsSuccess)
                {
                    TempData["success"] = "Registration Successful";
                    return RedirectToAction(nameof(Login));
                }
            }
            else
            {
                TempData["error"] = result.Message;
            }
            InItRoleDDl();
            return View(obj);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequestDto obj)
        {
            ResponseDto result = await _authService.LoginAsync(obj);

            if (result != null && result.IsSuccess)
            {
                LoginResponseDto loginResponseDto = JsonConvert.
                    DeserializeObject<LoginResponseDto>(Convert.ToString(result.Result));
                
                TempData["success"] = "Success!";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["error"] = result.Message;
                ModelState.AddModelError("CustomError", result.Message);
                return View(obj);
            }
        }

        void InItRoleDDl()
        {
            var roleList = new List<SelectListItem>() {
                new SelectListItem {Text = SD.RoleAdmin, Value = SD.RoleAdmin },
                new SelectListItem {Text = SD.RoleCustomer, Value = SD.RoleCustomer },
            };
            ViewBag.RoleList = roleList;
        }
    }
}
