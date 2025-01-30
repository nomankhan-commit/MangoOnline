using Mango.Services.EmailAPI.Message;
using Mango.Services.EmailAPI.Models.Dto;

namespace Mango.Services.EmailAPI.Services
{
    public interface IEmailService
    {
        Task EmailCartLog(CartDto cartDto);
        Task RegisterUserEmailandLog(string email);
        Task LogPlacedOrder(RewardsMessage rewardMessage);
    }
}
