using AutoMapper;
using Mango.Services.ProductAPI.Data;
using Mango.Services.ProductAPI.Models;
using Mango.Services.ProductAPI.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.ProductAPI.Controllers
{
    [Route("api/product")]
    [ApiController]
   // [Authorize]
    public class ProductAPIController : ControllerBase
    {
        private readonly AppDbContext _db;
        private ResponseDto _responseDto;
        private IMapper _mapper;
        public ProductAPIController(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _responseDto = new ResponseDto();
            _mapper = mapper;
        }
        
        [HttpGet]
        public object Get()
        {
            try
            {
                 IEnumerable<Product> obj = _db.Products.ToList();
                 _responseDto.Result = _mapper.Map<IEnumerable<ProductDto>>(obj);
            }
            catch (Exception ex)
            {

                _responseDto.IsSuccess = false;
                _responseDto.Result = ex.Message;

            }
            return _responseDto;
        }

        [HttpGet]
        [Route("{id:int}")]
        public object Get(int id) {

            try
            {
                Product obj = _db.Products.First(u=>u.ProductId==id);
                _responseDto.Result = _mapper.Map<ProductDto>(obj);
            }
            catch (Exception ex)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Result = ex.Message;
            }
            return _responseDto;
        }

        

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public object Post([FromBody] ProductDto productDto)
        {

            try
            {
                Product coupon = _mapper.Map<Product>(productDto);
                _db.Products.Add(coupon);
                _db.SaveChanges();
                _responseDto.Result = _mapper.Map<ProductDto>(productDto);
            }
            catch (Exception ex)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Result = ex.Message;
            }
            return _responseDto;
        }

        [HttpPut]
        [Authorize(Roles = "ADMIN")]
        public object Put([FromBody] ProductDto ProductDto)
        {

            try
            {
                Product coupon = _mapper.Map<Product>(ProductDto);
                _db.Products.Update(coupon);
                _db.SaveChanges();
                _responseDto.Result = _mapper.Map<ProductDto>(ProductDto);
            }
            catch (Exception ex)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Result = ex.Message;
            }
            return _responseDto;
        }

        [HttpDelete]
        [Route("{id:int}")]
        [Authorize(Roles = "ADMIN")]
        public object Delete(int id)
        {

            try
            {
                Product coupon = _db.Products.First(x => x.ProductId == id);
                _db.Remove(coupon);
                _db.SaveChanges();
                _responseDto.Result = _mapper.Map<ProductDto>(coupon);
            }
            catch (Exception ex)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Result = ex.Message;
            }
            return _responseDto;
        }
    }
}
