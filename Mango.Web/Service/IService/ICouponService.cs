using Mango.Web.Models;

namespace Mango.Web.Service.IService
{
    public interface ICouponService
    {
        Task<ResponseDto?> GetCouponAsync(string couponId); 
        Task<ResponseDto?> GetAllCouponAsync(); 
        Task<ResponseDto?> GetCouponByIdAsync(int id); 
        Task<ResponseDto?> CreateCouponsAsync(CouponDto couponDto); 
        Task<ResponseDto?> UpdateCouponAsync(CouponDto couponDto); 
        Task<ResponseDto?> DeleteCouponAsync(int id); 
    }
}
