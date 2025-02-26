﻿

namespace Mango.Services.OrderAPI.Models.Dto
{
    public class OrderHeaderDto
    {
        
        public int OrderHeaderId { get; set; }
        public string? UserId { get; set; }
        public string? CouponCode { get; set; }
        public double Discount { get; set; }
        public double OrderTotal { get; set; }
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public DateTime OrderTime { get; set; }
        public string? PaymentIntentId { get; set; }
        public string? StripSessionId { get; set; }
        public string? Status { get; set; }
        public IEnumerable<OrderDetailsDto> OrderDetails { get; set; }
    }
}
