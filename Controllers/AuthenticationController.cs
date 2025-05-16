using Astra.Services;
using Microsoft.AspNetCore.Mvc;
using Astra.Services.Interfaces;
using Astra.Dtos.Requests.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace Astra.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authsrv;

        public AuthenticationController(IAuthenticationService authsrv) => _authsrv = authsrv;

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {   
            // validate request body
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            // register a new user's account
            var result = await _authsrv.RegisterAccount(request);
            if (!result.IdentityResult.Succeeded) {
                return StatusCode((int)result.Code, result.IdentityResult.Errors);
            }

            return Ok(new { Message = "account created!, verification mail sent" });
        }

        [HttpPost]
        [Route("complete-registration")]
        public async Task<IActionResult> CompleteRegistration([FromBody] CompleteRegistrationRequest request) {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (result, response) = await _authsrv.CompleteRegistration(request);
            if (!result.IdentityResult.Succeeded && response != null) 
                return StatusCode((int)result.Code, result.IdentityResult.Errors);

            return Ok(response);
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (result, response) = await _authsrv.LoginAccount(request);
            if (!result.IdentityResult.Succeeded && response is null)
                return StatusCode((int)result.Code, result.IdentityResult.Errors);
            
            return Ok(response);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {   
            await _authsrv.Logout(User.Identity!);
            return NoContent();
        }

        [HttpPost]
        [Route("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _authsrv.ForgotPassword(request);
            if (!response.IdentityResult.Succeeded) 
                return StatusCode((int)response.Code, response.IdentityResult.Errors);

            return Ok(new {Message = "reset token sent"});
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _authsrv.ResetPassword(request);
            if (!response.IdentityResult.Succeeded) 
                return StatusCode((int)response.Code, response.IdentityResult.Errors);

            return Ok(new { Message = "password reset successfully" });
        }

        [HttpPost]
        [Route("send-confirmation-email")]
        public async Task<IActionResult> SendConfirmationEmail([FromBody] SendEmailConfirmationRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            var result = await _authsrv.SendConfirmationEmail(request);
            if (!result.IdentityResult.Succeeded) 
                return StatusCode((int)result.Code, result.IdentityResult.Errors);

            return Ok(new { Message = "email confirmation code sent" });
        }

        [HttpPost]
        [Route("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            var result = await _authsrv.VerifyConfirmationEmail(request);
            if (!result.IdentityResult.Succeeded) 
                return StatusCode((int)result.Code, result.IdentityResult.Errors);

            return Ok(new { Message = "email address verified" });
        }
    }
}