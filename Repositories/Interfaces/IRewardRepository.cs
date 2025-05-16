using Astra.Models;

namespace Astra.Repositories.Interfaces {
    public interface IRewardRepository {
        public Task Create(Reward reward);
        public Task RewardUser(Reward reward, int points);
        public Task<Reward?> Find(string UserID);
    }
}