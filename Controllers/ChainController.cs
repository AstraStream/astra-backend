using Microsoft.AspNetCore.Mvc;
using Astra.Services.Interfaces;
using Astra.Dtos.Queries.Chains;
using Astra.Dtos.Requests.Chains;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Astra.Controllers {
    [ApiController]
    [Route("chains")]
    public class ChainController: ControllerBase {
        private readonly IChainService _chainsrv;
        public ChainController(IChainService chainsrv) => _chainsrv = chainsrv;

        [HttpGet]
        public async Task<IActionResult> index([FromQuery] ChainQuery query) {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
                
            var chains = await _chainsrv.FindAllPaginated(query);
            return Ok(chains);
        }

        [HttpPost]
        public async Task<IActionResult> store([FromBody] StoreChainRequest request) {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _chainsrv.Create(request);
            if (!result.IdentityResult.Succeeded) 
                return StatusCode((int)result.Code, result.IdentityResult.Errors);

            return Ok(new { Message = "chain added!" });
        }

        [HttpGet]
        [Route("{uuid:guid}")]
        public async Task<IActionResult> show([FromRoute] Guid uuid) {
            var (result, response) = await _chainsrv.Find(uuid);
            if (!result.IdentityResult.Succeeded && response == null)
                return StatusCode((int)result.Code, result.IdentityResult.Errors);
            
            return Ok(response);
        }

        [HttpPut]
        [Route("{uuid:guid}")]
        public async Task<IActionResult> update([FromRoute] Guid uuid, [FromBody] UpdateChainRequest request) {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            var result = await _chainsrv.Update(uuid, request);
            if (!result.IdentityResult.Succeeded) 
                return StatusCode((int)result.Code, result.IdentityResult.Errors);

            return Ok(new { Message = "chain updated!" });
        }

        [HttpDelete]
        [Route("{uuid:guid}")]
        public async Task<IActionResult> destroy([FromRoute] Guid uuid) {
            var result = await _chainsrv.Delete(uuid);
            if (!result.IdentityResult.Succeeded) 
                return StatusCode((int)result.Code, result.IdentityResult.Errors);

            return Ok(new { Message = "chain deleted!" });
        }
    }
}