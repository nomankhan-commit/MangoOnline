using AutoMapper;
using Mango.Services.CouponAPI.Data;
using Mango.Services.CouponAPI.Models;
using Mango.Services.CouponAPI.Models.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.CouponAPI.Controllers
{
    [Route("api/coupon")]
    [ApiController]
    public class CouponAPIController : ControllerBase
    {
        private readonly AppDbContext _db;
        private ResponseDto _responseDto;
        private IMapper _mapper;
        public CouponAPIController(AppDbContext db, IMapper mapper)
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
                 IEnumerable<Coupon> obj = _db.Coupons.ToList();
                 _responseDto.Result = _mapper.Map<IEnumerable<CouponDto>>(obj);
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
                Coupon obj = _db.Coupons.First(u=>u.CouponId==id);
                _responseDto.Result = _mapper.Map<CouponDto>(obj);
            }
            catch (Exception ex)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Result = ex.Message;
            }
            return _responseDto;
        }

        [HttpGet]
        [Route("GetByCode/{code}")]
        public object GetByCode(string code)
        {

            try
            {
                Coupon obj = _db.Coupons.First(u => u.CouponCode.ToLower() == code);
                _responseDto.Result = _mapper.Map<CouponDto>(obj);
            }
            catch (Exception ex)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Result = ex.Message;
            }
            return _responseDto;
        }

        [HttpPost]
        public object Post([FromBody] CouponDto couponDto)
        {

            try
            {
                Coupon coupon = _mapper.Map<Coupon>(couponDto);
                _db.Coupons.Add(coupon);
                _db.SaveChanges();
                _responseDto.Result = _mapper.Map<CouponDto>(couponDto);
            }
            catch (Exception ex)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Result = ex.Message;
            }
            return _responseDto;
        }

        [HttpPut]
        public object Put([FromBody] CouponDto couponDto)
        {

            try
            {
                Coupon coupon = _mapper.Map<Coupon>(couponDto);
                _db.Coupons.Update(coupon);
                _db.SaveChanges();
                _responseDto.Result = _mapper.Map<CouponDto>(couponDto);
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
        public object Delete(int id)
        {

            try
            {
                Coupon coupon = _db.Coupons.First(x => x.CouponId == id);
                _db.Remove(coupon);
                _db.SaveChanges();
                _responseDto.Result = _mapper.Map<CouponDto>(coupon);
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
