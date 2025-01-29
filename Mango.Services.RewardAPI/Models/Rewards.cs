namespace Mango.Services.RewardAPI.Models
{
    public class Rewards
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime RewardDate { get; set; }
        public int RewardActivity { get; set; }
        public int OrderId { get; set; }
    }
}
