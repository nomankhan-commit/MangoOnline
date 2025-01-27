
using Mango.web.Model;
using Mango.web.Model.Dto;
using Mango.Web.Models;
using Mango.Web.Service;
using Mango.Web.Service.IService;
using Mango.Web.Utility;
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
        private readonly IOrderService _orderService;
        
        public CartController(ICartService couponService, IOrderService orderService)
        {
            _cartService = couponService;
            _orderService  = orderService;
        }


        [Authorize]
        public async Task<IActionResult> Index()
        {
            return View(await LoadCartDtoBasedonLoggedInUser());
        }

        [Authorize]
        public async Task<IActionResult> Confirmation(int orderId)
        {
            ResponseDto? response = await _orderService.ValidateStripeSession(orderId);
            if (response != null && response.IsSuccess)
            {
                OrderHeaderDto orderHeaderDto = JsonConvert.DeserializeObject<OrderHeaderDto>(Convert.ToString(response.Result));
                if (orderHeaderDto.Status == SD.Status_Approved)
                {
                    return View(orderId);
                }
            }
            return View(orderId);
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

        [HttpPost]
        public async Task<IActionResult> RemoveCoupon(CartDto cartDto)
        {
            cartDto.cartHeader.CouponCode = "";
            ResponseDto? responseDto = await _cartService.ApplyCouponAsync(cartDto);
            if (responseDto != null && responseDto.IsSuccess)
            {
                TempData["success"] = "Cart updated successfully.";
            }
            return RedirectToAction(nameof(Index));

        }

        [HttpPost]
        public async Task<IActionResult> ApplyCoupon(CartDto cartDto)
        {
            var userid = User.Claims.Where(c => c.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;
            ResponseDto? responseDto = await _cartService.ApplyCouponAsync(cartDto);
            if (responseDto != null && responseDto.IsSuccess)
            {
                TempData["success"] = "Cart updated successfully.";
            }
            return RedirectToAction(nameof(Index));

        }

        public async Task<IActionResult> Remove(int cartDetailId)
        {
            //var userid = User.Claims.Where(c => c.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;
            ResponseDto? responseDto = await _cartService.RemoveFromCartAsync(cartDetailId);
            if (responseDto != null && responseDto.IsSuccess)
            {
                TempData["success"] = "Cart updated successfully.";
            }
            return RedirectToAction(nameof(Index));

        }

        [Authorize]
        public async Task<IActionResult> Checkout() {

            CartDto cart = await LoadCartDtoBasedonLoggedInUser();
            return View(cart);
        }

        [HttpPost]
        [ActionName("Checkout")]
        public async Task<IActionResult> Checkout(CartDto cartDto)
        {

            CartDto cart = await LoadCartDtoBasedonLoggedInUser();
            cart.cartHeader.PhoneNumber = cartDto.cartHeader.PhoneNumber;
            cart.cartHeader.Email = cartDto.cartHeader.Email;
            cart.cartHeader.Name = cartDto.cartHeader.Name;

            var response = await _orderService.CreateOrder(cart);
            OrderHeaderDto orderHeaderDto =
                JsonConvert.DeserializeObject<OrderHeaderDto>(Convert.ToString(response.Result));
            var domain = Request.Scheme + "://" + Request.Host.Value + "/";
            if (response != null && response.IsSuccess)
            {
                StripeRequestDto stripeRequestDto = new StripeRequestDto()
                {
                    ApprovedUrl = domain + "/cart/confirmation?orderId="+orderHeaderDto.OrderHeaderId,
                    CancelUrl = domain + "cart/checkout",
                    OrderHeader = orderHeaderDto,   
                };
            var stripeResponse = await _orderService.CreateStripeSession(stripeRequestDto);
                StripeRequestDto stripeResponseResult =
                JsonConvert.DeserializeObject<StripeRequestDto>(Convert.ToString(stripeResponse.Result));
                Response.Headers.Add("Location",stripeResponseResult.StripeSessionUrl);//look
                return new StatusCodeResult(303);
            }
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> EmailCart(CartDto cartDto)
        {
            CartDto cart = await LoadCartDtoBasedonLoggedInUser();
            cart.cartHeader.Email = User.Claims.Where(c => c.Type == JwtRegisteredClaimNames.Email)?.FirstOrDefault()?.Value;
            ResponseDto? responseDto = await _cartService.EmailCart(cart);
            if (responseDto != null && responseDto.IsSuccess)
            {
                TempData["success"] = "Cart send to your email successfully.";
            }
            return RedirectToAction(nameof(Index));

        }
    }
}
