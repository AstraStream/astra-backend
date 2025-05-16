using Astra.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using ZiggyCreatures.Caching.Fusion;

namespace Astra.Common
{
    public class AuthUserHelper
    {
        private readonly UserManager<User> _userManager;
        private readonly IFusionCache _cache;

        public AuthUserHelper(UserManager<User> userManager, IFusionCache cache)
        {
            _userManager = userManager;
            _cache = cache;
        }

        public async Task<User> GetAuthenticatedUserAsync(ClaimsPrincipal principal)
        {
            string email = principal.FindFirstValue(ClaimTypes.Email)!;
            var user = await _cache.GetOrSetAsync(
                $"auth:{email}",
                async _ => {
                    var response = await _userManager.FindByEmailAsync(email);
                    return response;
                },
                options => options.SetDuration(TimeSpan.FromMinutes(5)).SetFailSafe(true)
            );
            return user;
        }
    }
}