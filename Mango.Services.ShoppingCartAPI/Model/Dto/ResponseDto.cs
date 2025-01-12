namespace Mango.Services.ShoppingCartAPI.Model.Dto
{
    public class ResponseDto
    {
        public object Result { get; set; }
        public bool Sussess { get; set; } = true;
        public string Message { get; set; } = string.Empty;
    }
}
