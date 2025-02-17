using AutoMapper;
using Mango.Services.ShoppingCartAPI.Model.Dto;
using Mango.Services.ShoppingCartAPI.Model;
using Microsoft.AspNetCore.Mvc;
using Mango.Service.ShoppingCart.Data;
using Microsoft.EntityFrameworkCore;
using Mango.Services.ShoppingCartAPI.Models.Dto;
using Mango.Services.ShoppingCartAPI.Service.IService;
using Mango.MessageBus;
using Mango.Services.ShoppingCartAPI.RabbitMQSender;

namespace Mango.Services.ShoppingCartAPI.Controllers
{
    [Route("api/cart")]
    [ApiController]
    public class CartAPIController :  ControllerBase
    {
        private readonly ResponseDto _responseDto;
        private IMapper _mapper;
        private readonly AppDbContext _db;
        private readonly IConfiguration _configuration;
        private readonly IProductService _productService;
        private readonly ICouponService _couponService;
        //private readonly IMessageBus _messageBus;
        private readonly IRabbitMQAuthMessageSender _messageBus;
        public CartAPIController(AppDbContext db, IMapper mapper, IProductService productService, 
            ICouponService couponService, IRabbitMQAuthMessageSender messageBus, IConfiguration configuration
            )
        {
            _db = db;
            _responseDto = new ResponseDto();
            _mapper = mapper;
            _productService = productService;
            _couponService = couponService;
            _messageBus = messageBus;
            _configuration = configuration;
        }

        [HttpGet("GetCart/{userId}")]
        public async Task<ResponseDto> GetCart(string userId)
        {
            try
            {
               

                CartDto cart = new()
                {
                    cartHeader = _mapper.Map<CartHeaderDto>(_db.cartHeaders.FirstOrDefault(x => x.UserId == userId))
                };
                cart.cartDetails = _mapper.Map<IEnumerable<CartDetailsDto>>(_db.
                    cartDetails.Where(x => x.CartHeaderId == cart.cartHeader.CartHeaderId));
                IEnumerable<ProductDto> productDtos = await _productService.GetProduct();

                foreach (var item in cart.cartDetails)
                {
                    item.ProductDto = productDtos.FirstOrDefault(x => x.ProductId == item.ProductId);
                    cart.cartHeader.CartTotal += (item.Count * item.ProductDto.Price);
                }

                //apply coupon if any
                if (!string.IsNullOrEmpty(cart.cartHeader.CouponCode))
                {
                    CouponDto couponDto = await _couponService.GetCoupon(cart.cartHeader.CouponCode);
                    if (couponDto != null && cart.cartHeader.CartTotal > couponDto.MinAmount)
                    {
                        cart.cartHeader.CartTotal -= couponDto.DiscountAmount;
                        cart.cartHeader.Discount = couponDto.DiscountAmount;

                    }
                }

                _responseDto.Result = cart;
                _responseDto.Sussess = true;
            }
            catch (Exception ex)
            {

                _responseDto.Sussess = false;
                _responseDto.Message = ex.Message;
            }
            return _responseDto;
        }

        [HttpPost("CartUpsert")]
        public async Task<ResponseDto> CartUpsert(CartDto cartDto)
        {
            try
            {
                var cartHeaderFromDb = await _db.cartHeaders.FirstOrDefaultAsync(x => x.UserId == cartDto.cartHeader.UserId);
                if (cartHeaderFromDb == null)
                {

                    // cart header and details
                    CartHeader cartHeader = _mapper.Map<CartHeader>(cartDto.cartHeader);
                    _db.cartHeaders.Add(cartHeader);
                    await _db.SaveChangesAsync();

                    cartDto.cartDetails.First().CartHeaderId = cartHeader.CartHeaderId;
                    CartDetails cartDetails = _mapper.Map<CartDetails>(cartDto.cartDetails.First());
                    _db.cartDetails.Add(cartDetails);
                    await _db.SaveChangesAsync();
                }
                else
                {

                    //if header is not null.
                    //check if details has some product.
                    var cartDetailsFromDb = await _db.cartDetails.AsNoTracking().FirstOrDefaultAsync(
                        x => x.ProductId== cartDto.cartDetails.First().ProductId && 
                        x.CartHeaderId == cartHeaderFromDb.CartHeaderId);
                    if (cartDetailsFromDb == null)
                    {
                        // create card details.
                        cartDto.cartDetails.First().CartHeaderId = cartHeaderFromDb.CartHeaderId;
                        CartDetails cartDetails = _mapper.Map<CartDetails>(cartDto.cartDetails.First());
                        _db.cartDetails.Add(cartDetails);
                        await _db.SaveChangesAsync();

                    }
                    else
                    {
                        // update cartdetails.
                        cartDto.cartDetails.First().Count += cartDetailsFromDb.Count;
                        cartDto.cartDetails.First().CartHeaderId = cartDetailsFromDb.CartHeaderId;
                        cartDto.cartDetails.First().CartDetailsId = cartDetailsFromDb.CartDetailsId;
                        _db.cartDetails.Update(_mapper.Map<CartDetails>(cartDto.cartDetails.First()));
                        await _db.SaveChangesAsync();
                    }
                }
                _responseDto.Result = cartDto;
            }
            catch (Exception ex)
            {

                _responseDto.Sussess = false;
                _responseDto.Message = ex.Message;
            }
            return _responseDto;
        }

        [HttpPost("RemoveCart")]
        public async Task<ResponseDto> RemoveCart([FromBody] int cartDetailsId)
        {
            try
            {

                CartDetails cartDetails1 = _db.cartDetails.First(x => x.CartDetailsId == cartDetailsId);
                var totalCountofCartItems = _db.cartDetails.Where(x => x.CartHeaderId == cartDetails1.CartHeaderId).Count();
                _db.cartDetails.Remove(cartDetails1);
                if (totalCountofCartItems == 1)
                {
                    var cartHeaderToRemove = await _db.cartHeaders
                        .FirstOrDefaultAsync(x => x.CartHeaderId == cartDetails1.CartHeaderId);
                    _db.cartHeaders.Remove(cartHeaderToRemove);
                }
                await _db.SaveChangesAsync();
                _responseDto.Sussess = true;
                _responseDto.Result = null;
            }
            catch (Exception ex)
            {

                _responseDto.Sussess = false;
                _responseDto.Message = ex.Message;
            }
            return _responseDto;
        }
        
        [HttpPost("ApplyCoupon")]
        public async Task<ResponseDto> ApplyCoupon(CartDto cartDto)
        {
            try
            {
                var cartFromDb = await _db.cartHeaders.FirstAsync(o => o.UserId == cartDto.cartHeader.UserId);
                cartFromDb.CouponCode = cartDto.cartHeader.CouponCode;
                _db.cartHeaders.Update(cartFromDb);
                await _db.SaveChangesAsync();
                _responseDto.Result = true;
                _responseDto.Sussess = true;
            }
            catch (Exception ex)
            {

                _responseDto.Sussess = false;
                _responseDto.Message = ex.Message;
            }
            return _responseDto;
        }

        [HttpPost("EmailCartRequest")]
        public async Task<ResponseDto> EmailCartRequest(CartDto cartDto)
        {
            try
            {
                 _messageBus.SendMessage(cartDto, _configuration.GetValue<string>("TopicsAndQueueName:EmailShoppingCartQueue"));
                _responseDto.Result = true;
                _responseDto.Sussess = true;
            }
            catch (Exception ex)
            {

                _responseDto.Sussess = false;
                _responseDto.Message = ex.Message;
            }
            return _responseDto;
        }
    }
}
