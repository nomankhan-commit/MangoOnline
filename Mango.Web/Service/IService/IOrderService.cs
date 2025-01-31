using Mango.Web.Models;

namespace Mango.Web.Service.IService
{
    public interface IOrderService
    {
        Task<ResponseDto?> CreateOrder(CartDto cartDto);
        Task<ResponseDto?> CreateStripeSession(StripeRequestDto stripeRequestDto);
        Task<ResponseDto?> ValidateStripeSession(int orderHeaderId);
        
        Task<ResponseDto?> UpdateOrderStatus(int orderid, string newStatus);
        Task<ResponseDto?> GetAllOrders(string? userid);
        Task<ResponseDto?> GetOrder(string? orderid);

    }
}
