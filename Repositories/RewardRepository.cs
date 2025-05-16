using Astra.Models;
using Astra.Database;
using Astra.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Astra.Repositories {
    public class RewardRepository: IRewardRepository {
        private readonly AstraDbContext _context;
        public RewardRepository(AstraDbContext context) => _context = context;

        public async Task Create(Reward reward) {
            await _context.Rewards.AddAsync(reward);
            await _context.SaveChangesAsync();
        }

        public async Task RewardUser(Reward reward, int points) {
            reward.Points = points;
            reward.LastEarnedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<Reward?> Find(string userID) {
            return await _context.Rewards
                    .FirstOrDefaultAsync(r => r.UserId == userID);
        }
    }
}