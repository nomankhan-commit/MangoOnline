using Mango.Services.RewardAPI.Data;
using Mango.Services.RewardAPI.Message;
using Mango.Services.RewardAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Mango.Services.RewardAPI.Services
{
    public class RewardService : IRewardService
    {
        private DbContextOptions<AppDbContext> options;
        public RewardService(DbContextOptions<AppDbContext> options)
        {
            this.options = options;
        }

        
        public async Task UpdateRewards(RewardsMessage rewardsMessage)
        {
            try
            {
                Rewards rewards = new()
                {
                    OrderId = rewardsMessage.OrderId,
                    RewardActivity = rewardsMessage.RewardActivity,
                    UserId = rewardsMessage.UserId,
                    RewardDate = DateTime.Now,
                };

                await using var db = new AppDbContext(options);
                db.Add(rewards);
                await db.SaveChangesAsync();

            }
            catch (Exception ex)
            {

            }
        }
       

    }
}
