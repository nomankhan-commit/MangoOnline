using Mango.Services.RewardAPI.Message;
using Mango.Services.RewardAPI.Models;

namespace Mango.Services.RewardAPI.Services
{
    public interface IRewardService
    {
        Task UpdateRewards(RewardsMessage rewardsMessage);
    }
}
