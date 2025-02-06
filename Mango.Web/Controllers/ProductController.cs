using Mango.Web.Models;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Mango.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        public ProductController(IProductService productService)
        {

            _productService = productService;
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
        
        public async Task<IActionResult> Create()
        {

            return View();
        }
        public async Task<IActionResult> Delete(int productid)
        {

            ProductDto Product = new();
            ResponseDto? responseDto = await _productService.GetProductByIdAsync(productid);
            if (responseDto != null && responseDto.IsSuccess)
            {
                Product = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(responseDto.Result));
            }
            return View(Product);
        }
        public async Task<IActionResult> Edit(int productid)
        {

            ProductDto Product = new();
            ResponseDto? responseDto = await _productService.GetProductByIdAsync(productid);
            if (responseDto != null && responseDto.IsSuccess)
            {
                Product = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(responseDto.Result));
            }
            return View(Product);
        }

        [HttpPost]
        public async Task<IActionResult> ProductCreate(ProductDto model)
        {

            if (ModelState.IsValid)
            {
                List<ProductDto> List = new();
                ResponseDto? responseDto = await _productService.CreateProductAsync(model);
                if (responseDto != null && responseDto.IsSuccess)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["error"] = responseDto.Message;
                }
            }

            return View(nameof(Create), model);
        }

        [HttpPost]
        public async Task<IActionResult> ProductEdit(ProductDto model)
        {
            if (ModelState.IsValid)
            {
                ProductDto Product = new();
                ResponseDto? responseDto = await _productService.UpdateProductAsync(model);
                if (responseDto != null && responseDto.IsSuccess)
                {
                    TempData["success"] = "Product updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["error"] = responseDto.Message;
                    
                }
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> ProductDelete(ProductDto model)
        {

            ProductDto Product = new();
            ResponseDto? responseDto = await _productService.DeleteProductAsync(model.ProductId);
            if (responseDto != null && responseDto.IsSuccess)
            {
                TempData["success"] = "Product deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["error"] = responseDto.Message;
                return View(nameof(Delete),model);
            }
        }

    }
}
