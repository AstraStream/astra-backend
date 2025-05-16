using Astra.Dtos.Entities;

namespace Astra.Services.Interfaces {
    public interface IRewardService {
        public Task Create(string userID);
        public Task<ServiceResult> RewardUser(string userID, int points);
    }
}