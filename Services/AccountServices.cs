using QRCoder;
using Mapster;
using System.Net;
using System.Text;
using Astra.Models;
using Astra.Common;
using Astra.Dtos.Entities;
using System.Security.Claims;
using Astra.Services.Interfaces;
using Astra.Dtos.Responses.Users;
using Microsoft.AspNetCore.Identity;
using ZiggyCreatures.Caching.Fusion;

namespace Astra.Services {
    public class AccountService: IAccountService {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<AccountService> _logger;
        private readonly IFusionCache _cache;
        private readonly AuthUserHelper _authhelper;

        public AccountService(
            UserManager<User> userManager,
            ILogger<AccountService> logger,
            AuthUserHelper authhelper,
            IFusionCache cache
        )
        {
            _cache = cache;
            _logger = logger;
            _authhelper = authhelper;
            _userManager = userManager;
        }

        public async Task<UserResponse> GetAuthUser(ClaimsPrincipal principal) {
            _logger.LogInformation("retrieve user information and cache response");
            var response = await _authhelper.GetAuthenticatedUserAsync(principal)!;
            var roles = await _userManager.GetRolesAsync(response!);

            var user = response.Adapt<UserResponse>();
            user.Roles = roles.ToList();
            return user;
        }

        public async Task<(ServiceResult, byte[]?)> Setup2FA(ClaimsPrincipal principal) {
            _logger.LogInformation("retrieve user information and cache response");
            var user = await _authhelper.GetAuthenticatedUserAsync(principal)!;
            
            if (user == null)
            {
                return (
                    new ServiceResult
                    {
                        Code = HttpStatusCode.Unauthorized,
                        IdentityResult = IdentityResult.Failed(new IdentityError
                        {
                            Code = "UserNotFound",
                            Description = "user account not found"
                        })
                    }, null);
            }

            _logger.LogInformation("Generating TOTP secret");
            var totpSecret = Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));

            user.TotpSecret = totpSecret;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) {
                _logger.LogError("there was an error updating user's data");
                return (
                    new ServiceResult {
                        Code = HttpStatusCode.InternalServerError,
                        IdentityResult = IdentityResult.Failed(new IdentityError {
                            Code = "2FAUpdateError",
                            Description = "could not update user data"
                        })
                    }, null
                );
            }

            _logger.LogInformation("Build TOTP URI");
            var issuer = "astra";
            var totpUri = $"otpauth://totp/{issuer}:{user.Email}?secret={totpSecret}&issuer={issuer}";

            _logger.LogInformation("Generate QRCode");
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(totpUri, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeBytes = qrCode.GetGraphic(20);

            return (
                new ServiceResult {
                    Code = HttpStatusCode.OK,
                    IdentityResult = IdentityResult.Success
                }, qrCodeBytes);
        }

        public async Task Verify2FA(ClaimsPrincipal principal) {
            _logger.LogInformation("retrieve user information and cache response");
            string email = principal.FindFirstValue(ClaimTypes.Email)!;
            var user = await _cache.GetOrSetAsync(
                $"user:{email}",
                async _ => await _userManager.FindByEmailAsync(email),
                options => options.SetDuration(TimeSpan.FromMinutes(5)).SetFailSafe(true)
            );

            
        }
    }
}
