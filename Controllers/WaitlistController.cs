using Microsoft.AspNetCore.Mvc;
using Astra.Services.Interfaces;
using Astra.Dtos.Queries.Waitlists;
using Astra.Dtos.Requests.Waitlists;

namespace Astra.Controllers {
    [ApiController]
    [Route("waitlists")]
    public class WaitlistController: ControllerBase {
        private readonly IWaitlistService _waitlistsrv;

        public WaitlistController(IWaitlistService waitlistsrv) => _waitlistsrv = waitlistsrv;

        [HttpGet]
        public async Task<IActionResult> index([FromQuery] WaitlistQuery query) {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var waitlists = await _waitlistsrv.FindAll(query);
            return Ok(waitlists);
        }

        [HttpPost]
        public async Task<IActionResult> store([FromBody] StoreWaitlistRequest request) {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _waitlistsrv.Create(request);
            if (!result.IdentityResult.Succeeded)
                return StatusCode((int)result.Code, result.IdentityResult.Errors);
            
            return Ok(new { Message = "added to waitlist" });
        }
    }
}