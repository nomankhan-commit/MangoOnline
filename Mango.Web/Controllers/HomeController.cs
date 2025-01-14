using Mango.Web.Models;
using Mango.Web.Service;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;

namespace Mango.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICartService _cartService;

        public HomeController(IProductService productService, ICartService cartService)
        {
            _productService = productService;
            _cartService = cartService;
        }

        public async Task<IActionResult> Index()
        {
            List<ProductDto> List = new();
            ResponseDto? responseDto = await _productService.GetAllProductAsync();
            if (responseDto != null && responseDto.IsSuccess)
            {
                List = JsonConvert.DeserializeObject<List<ProductDto>>(Convert.ToString(responseDto.Result));
            }
            else
            {
                TempData["error"] = responseDto.Message;
            }
            return View(List);
        }

        [Authorize]
        public async Task<IActionResult> ProductDetails(int productid)
        {
            ProductDto Product = new();
            ResponseDto? responseDto = await _productService.GetProductByIdAsync(productid);
            if (responseDto != null && responseDto.IsSuccess)
            {
                Product = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(responseDto.Result));
            }
            return View(Product);
        }




        [Authorize]
        [HttpPost]
        [ActionName("ProductDetails")]
        public async Task<IActionResult> ProductDetails(ProductDto productDto)
        {

            CartDto cartDto = new CartDto()
            {
                cartHeader = new CartHeaderDto()
                {
                    UserId = User.Claims.Where(c => c.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value
                }
            };

            CartDetailsDto cartDetails = new CartDetailsDto()
            {
                Count = productDto.Count,
                ProductId = productDto.ProductId
            };

            //List<CartDetailsDto> cartDetailsDtos = new List<CartDetailsDto>();
            List<CartDetailsDto> cartDetailsDtos = new() { cartDetails };
            cartDto.cartDetails = cartDetailsDtos;


            ResponseDto? responseDto = await _cartService.UpSertCartAsync(cartDto);
            if (responseDto != null && responseDto.IsSuccess)
            {
                TempData["success"] = "Items has been added to the shopping cart.";
                return RedirectToAction(nameof(Index));
            }
            else
            {

                TempData["error"] = responseDto.Message;
                return RedirectToAction("Details", new { productId = productDto.ProductId });
            }
            //
        }



        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
