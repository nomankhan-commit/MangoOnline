using AutoMapper;
using Mango.MessageBus;
using Mango.Services.OrderAPI.Models.Dto;
using Mango.Services.OrderAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mango.Services.OrderAPI.Data;
using Mango.Services.OrderAPI.Service.IService;
using Mango.Services.OrderAPI.Utility;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Mango.Web.Models;
using System.Runtime.CompilerServices;
using Stripe.Checkout;

namespace Mango.Services.OrderAPI.Controllers
{
    [Route("api/order")]
    [ApiController]
    public class OrderController : Controller
    {


        protected ResponseDto _response;
        private IMapper _mapper;
        private readonly AppDbContext _db;
        private IProductService _productService;
        private IMessageBus _messageBus;
        private IConfiguration _configuration;
        public OrderController(AppDbContext appDbContext,
            IProductService productService, IMapper mapper,
            IMessageBus messageBus, IConfiguration configuration)
        {
            _mapper = mapper;
            _db = appDbContext;
            _productService = productService;
            _messageBus = messageBus;
            _configuration = configuration;
            _response = new ResponseDto();
        }

        [Authorize]
        [HttpGet("GetOrders")]
        public ResponseDto? Get(string? userid = "")
        {
            try
            {
                IEnumerable<OrderHeader> objlist;
                if (User.IsInRole(SD.RoleAdmin))
                {
                    objlist = _db.OrderHeaders
                        .Include(x => x.OrderDetails)
                        .OrderByDescending(x => x.OrderHeaderId).ToList();
                }
                else
                {
                    objlist = _db.OrderHeaders
                        .Include(x => x.OrderDetails)
                        .Where(x => x.UserId == userid)
                        .OrderByDescending(x => x.OrderHeaderId).ToList();
                }
                _response.Result = _mapper.Map<IEnumerable<OrderHeaderDto>>(objlist);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;

            }
            return _response;
        }

        [Authorize]
        [HttpGet("GetOrder/{id:int}")]
        public ResponseDto? Get(int OrderHeaderId)
        {
            try
            {
                OrderHeader orderHeader = _db.OrderHeaders.Include(x => x.OrderDetails).First(x => x.OrderHeaderId == OrderHeaderId);
                _response.Result = _mapper.Map<OrderHeaderDto>(orderHeader);
                _response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;

            }
            return _response;
        }

        [Authorize]
        [HttpPost("CreateOrder")]
        public async Task<ResponseDto> CreateOrder([FromBody] CartDto cartDto)
        {

            try
            {
                //[FromBody]
                OrderHeaderDto orderHeaderDto = _mapper.Map<OrderHeaderDto>(cartDto.cartHeader);
                orderHeaderDto.OrderTime = DateTime.Now;
                orderHeaderDto.Status = SD.Status_Pending;
                orderHeaderDto.OrderDetails = _mapper.Map<IEnumerable<OrderDetailsDto>>(cartDto.cartDetails);

                OrderHeader orderCreated =  _db.OrderHeaders.Add(_mapper.Map<OrderHeader>(orderHeaderDto)).Entity;//look
                 await _db.SaveChangesAsync();

                orderHeaderDto.OrderHeaderId = orderCreated.OrderHeaderId;
                _response.Result= orderHeaderDto;
               
            }
            catch (Exception ex)
            {
                _response.Result = ex.Message;
                _response.IsSuccess = false;
            }
            return _response;
        }

        [Authorize]
        [HttpPost("CreateStripeSession")]
        public async Task<ResponseDto> CreateStripeSession([FromBody] StripeRequestDto stripeRequestDto) {

            try
            {
                

                var options = new Stripe.Checkout.SessionCreateOptions
                {
                    SuccessUrl = stripeRequestDto.ApprovedUrl,
                    CancelUrl = stripeRequestDto.CancelUrl,
                    LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
                    Mode = "payment",
                };

                var discountsObj = new List<SessionDiscountOptions>()
                {
                    new SessionDiscountOptions 
                    {
                        Coupon = stripeRequestDto.OrderHeader.CouponCode
                    }
                };

                foreach (var item in stripeRequestDto.OrderHeader.OrderDetails)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.ProductPrice * 100),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.ProductDto.ProductName,
                            }

                        },
                        Quantity = item.Count
                    };
                    options.LineItems.Add(sessionLineItem);
                }

                if (stripeRequestDto.OrderHeader.Discount > 0)
                {
                    options.Discounts = discountsObj;
                }
                var service = new Stripe.Checkout.SessionService();
                Session session = service.Create(options);
                stripeRequestDto.StripeSessionUrl = session.Url;
                OrderHeader orderHeader = _db.OrderHeaders.First(u => u.OrderHeaderId == stripeRequestDto.OrderHeader.OrderHeaderId);
                orderHeader.StripSessionId = session.Id;
                _db.SaveChanges();
                _response.Result = stripeRequestDto;
            }
            catch (Exception ex)
            {
                _response.Message = ex.Message; 
                _response.IsSuccess = false;
            }
            return _response;
        }

        [Authorize]
        [HttpPost("ValidateStripeSession")]
        public async Task<ResponseDto> ValidateStripeSession([FromBody] int orderHeaderId)
        {

            try
            {

                OrderHeader orderHeader = _db.OrderHeaders.First(u=>u.OrderHeaderId == orderHeaderId);
                var service = new Stripe.Checkout.SessionService();
                Session session = service.Get(orderHeader.StripSessionId);

                var paymentIntentService = new PaymentIntentService();
                PaymentIntent paymentIntent = paymentIntentService.Get(session.PaymentIntentId);
                if (paymentIntent.Status == "succeeded")
                {
                    orderHeader.PaymentIntentId = paymentIntent.Id;
                    orderHeader.Status = SD.Status_Approved;
                    _db.SaveChanges();
                    RewardsDto rewardsDto = new()
                    {

                        OrderId = orderHeaderId,
                        RewardsActivity = Convert.ToInt32(orderHeader.OrderTotal),
                        UserId =  orderHeader.UserId
                    };
                    string topicname = _configuration.GetValue<string>("TopicsAndQueueName:OrderCreatedTopic");
                    await _messageBus.PublishMessage(rewardsDto,topicname);
                    _response.Result = _mapper.Map<OrderHeaderDto>(orderHeader);
                }

             
            }
            catch (Exception ex)
            {
                _response.Message = ex.Message;
                _response.IsSuccess = false;
            }
            return _response;
        }


        [Authorize]
        [HttpPost("UpdateOrderStatus/{orderId:int}")]
        public async Task<ResponseDto> UpdateOrderStatus(int orderId, [FromBody] string newStatus)
        {

            try
            {
                OrderHeader orderHeader = _db.OrderHeaders.First(u=>u.OrderHeaderId == orderId);
                if (orderHeader != null) {
                    if (newStatus == SD.Status_Cancelled)
                    {
                        var options = new RefundCreateOptions { 
                            Reason = RefundReasons.RequestedByCustomer,
                            PaymentIntent = orderHeader.PaymentIntentId
                        };
                        var service = new RefundService();
                        Refund refund = service.Create(options);
                    }
                     orderHeader.Status = newStatus;
                    _db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _response.Message = ex.Message;
                _response.IsSuccess = false;
            }
            return _response;
        }

    }
}
