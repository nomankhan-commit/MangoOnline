

namespace Mango.Web.Models
{
    public class CartDto
    {
        public CartHeaderDto cartHeader { get; set; }
        public IEnumerable<CartDetailsDto>? cartDetails { get; set; }
    }
}
