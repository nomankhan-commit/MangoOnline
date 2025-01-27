using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utility;
using System.Runtime.CompilerServices;

namespace Mango.Web.Service
{
    public class OrderService : IOrderService
    {

        private readonly IBaseService _baseService;
        public OrderService(IBaseService baseService)
        {

            _baseService = baseService;

        }

        public async Task<ResponseDto?> CreateOrder(CartDto cartDto)
        {
            return await _baseService.SendAsync(new RequestDto()
            {

                ApiType = SD.ApiType.POST,
                Data = cartDto,
                Url = SD.OrderApiBase + "/api/Order/CreateOrder/"

            });
        }

        public async Task<ResponseDto?> CreateStripeSession(StripeRequestDto stripeRequestDto)
        {
            return await _baseService.SendAsync(new RequestDto()
            {

                ApiType = SD.ApiType.POST,
                Data = stripeRequestDto,
                Url = SD.OrderApiBase + "/api/Order/CreateStripeSession/"

            });
        }

        public async Task<ResponseDto?> GetAllOrders(string? userid)
        {
            return await _baseService.SendAsync(new RequestDto()
            {

                ApiType = SD.ApiType.GET,
                Url = SD.OrderApiBase + "/api/Order/GetOrders/" + userid

            });
        }

        public async Task<ResponseDto?> GetOrder(string? orderid)
        {
            return await _baseService.SendAsync(new RequestDto()
            {

                ApiType = SD.ApiType.GET,
                Url = SD.OrderApiBase + "/api/Order/GetOrder/" + orderid

            });
        }

        public async Task<ResponseDto?> UpdateOrderStatus(int orderid, string newStatus)
        {
            return await _baseService.SendAsync(new RequestDto()
            {

                ApiType = SD.ApiType.POST,
                Data = newStatus,
                Url = SD.OrderApiBase + "/api/Order/UpdateOrderStatus/" + orderid

            });
        }

        public async Task<ResponseDto?> ValidateStripeSession(int orderHeaderId)
        {
            //
            return await _baseService.SendAsync(new RequestDto()
            {

                ApiType = SD.ApiType.POST,
                Data = orderHeaderId,
                Url = SD.OrderApiBase + "/api/Order/ValidateStripeSession/"

            });
        }
    }
}
