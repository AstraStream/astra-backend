using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Astra.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Astra.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("accounts")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountsrv;
        public AccountController(IAccountService accountsrv) => _accountsrv = accountsrv;

        [HttpGet]
        [Route("profile")]
        public async Task<IActionResult> Profile() {
            var user = await _accountsrv.GetAuthUser(User);
            return Ok(user);
        }

        [HttpPost]
        [Route("setup-2fa")]
        public async Task<IActionResult> Setup2FA() {
            var (result, qrcode) = await _accountsrv.Setup2FA(User);
            if (!result.IdentityResult.Succeeded && qrcode == null)
                return StatusCode((int)result.Code, result.IdentityResult.Errors);

            return File(qrcode!, "image/png");
        }
    }
}