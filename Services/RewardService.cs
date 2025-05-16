using System.Net;
using Astra.Models;
using Astra.Dtos.Entities;
using Astra.Services.Interfaces;
using ZiggyCreatures.Caching.Fusion;
using Microsoft.AspNetCore.Identity;
using Astra.Repositories.Interfaces;

namespace Astra.Services {
    public class RewardService: IRewardService {
        private readonly IConfiguration _config;
        private readonly ILogger<RewardService> _logger;
        private readonly IRewardRepository _rewardrepo;
        private readonly IFusionCache _cache;

        public RewardService(
            IFusionCache cache,
            IConfiguration config,
            ILogger<RewardService> logger,
            IRewardRepository rewardrepo
        ) {
            _cache = cache;
            _config = config;
            _logger = logger;
            _rewardrepo = rewardrepo;
        }

        public async Task Create(string userID) {
            _logger.LogInformation("Rewarding new user");
            var reward = new Reward { 
                UUID = Guid.NewGuid(),
                UserId = userID, 
                Points = _config.GetValue<int>("Rewards:OnboardPoints"),
                LastEarnedAt = DateTime.UtcNow,
            };
            await _rewardrepo.Create(reward);
        }

        public async Task<ServiceResult> RewardUser(string userID, int points) {
            _logger.LogInformation("retrieve user rewards data");
            var reward = await _cache.GetOrSetAsync(
                $"reward:{userID}",
                async _ => await _rewardrepo.Find(userID),
                options => options.SetDuration(TimeSpan.FromSeconds(40)).SetFailSafe(true)
            );

            if (reward == null) {
                _logger.LogError("reward record not found in database");
                return new ServiceResult {
                    Code = HttpStatusCode.NotFound,
                    IdentityResult = IdentityResult.Failed(new IdentityError {
                        Code = "RewardNotFound",
                        Description = "reward record not found"
                    })
                };
            }

            try {
                // update necessary column
                _logger.LogInformation("updating user's reward record");
                await _rewardrepo.RewardUser(reward, points);

                return new ServiceResult {
                    Code = HttpStatusCode.OK,
                    IdentityResult = IdentityResult.Success
                };

            } catch (Exception e) {
                _logger.LogError($"could not add point {e}");
                return new ServiceResult {
                    Code = HttpStatusCode.OK,
                    IdentityResult = IdentityResult.Failed(new IdentityError {
                        Code = "RewardUpdateError",
                        Description = "encountered an error updating chain"
                    })
                };
            }
        }
    }
}