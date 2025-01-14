
using Mango.Web.Models;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;

namespace Mango.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        
        public CartController(ICartService couponService)
        {
            _cartService = couponService;
        }


        [Authorize]
        public async Task<IActionResult> Index()
        {
            return View(await LoadCartDtoBasedonLoggedInUser());
        }
        public async Task<CartDto> LoadCartDtoBasedonLoggedInUser()
        {

            var userid = User.Claims.Where(c => c.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;
            ResponseDto? responseDto = await _cartService.GetCartByUserIdAsync(userid);
            if (responseDto != null && responseDto.IsSuccess)
            { 
                CartDto cartDto = JsonConvert.DeserializeObject<CartDto>(Convert.ToString(responseDto.Result));
                return cartDto;
            }
            return new CartDto();
        }
    }
}
