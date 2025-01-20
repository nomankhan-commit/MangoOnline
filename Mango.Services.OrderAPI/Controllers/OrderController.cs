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
        [HttpPost("UpdateOrderStatus/{orderid:int}")]
        public ResponseDto? UpdateOrderStatus(int orderid, [FromBody] string newStatus)
        {
            try
            {
                OrderHeader orderHeader = _db.OrderHeaders.First(x => x.OrderHeaderId == orderid);
                if (orderHeader == null)
                {


                    if (newStatus == SD.Status_Cancelled)
                    {

                    }
                    orderHeader.Status = newStatus;
                    _db.SaveChanges();
                }

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

    }
}
