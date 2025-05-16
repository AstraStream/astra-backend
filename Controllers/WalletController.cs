using Microsoft.AspNetCore.Mvc;
using Astra.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Astra.Controllers {
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("wallets")]
    public class WalletController: ControllerBase {
        private readonly IWalletService _walletsrv;

        public WalletController(IWalletService walletsrv) {
            _walletsrv = walletsrv;
        }

        [HttpGet]
        public async Task<IActionResult> index() {
            var wallets = await _walletsrv.FindAllByUser(User);
            return Ok(wallets);
        }

        [HttpGet("{chainID:Guid}")]
        public async Task<IActionResult> show([FromRoute] Guid chainID) {
            var wallet = await _walletsrv.FindByChainAndUser(User, chainID);
            return Ok(wallet);
        }

        [HttpGet("{address}/balance")]
        public async Task<IActionResult> balance([FromRoute] string address) {
            return Ok(await _walletsrv.FetchSolanaBalance(address));
        }
    }
}