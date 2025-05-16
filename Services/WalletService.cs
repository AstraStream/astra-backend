using Mapster;
using Astra.Models;
// using Astra.Common;
using Astra.Database;
using System.Security.Claims;
using Astra.Services.Interfaces;
using Astra.Dtos.Responses.Wallets;
using Astra.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using ZiggyCreatures.Caching.Fusion;

namespace Astra.Services
{
    public class WalletService : IWalletService
    {
        private readonly AstraDbContext _context;
        private readonly IChainRepository _chainrepo;
        private readonly IWalletRepository _walletrepo;
        private readonly UserManager<User> _userManager;
        private readonly ISolanaService _solanasrv;
        private readonly IFusionCache _cache;
        private readonly IEncryptionService _encryptionsrv;
        private readonly ILogger<WalletService> _logger;

        public WalletService(
            IFusionCache cache,
            AstraDbContext context,
            IWalletRepository walletrepo,
            IChainRepository chainRepository,
            UserManager<User> userManager,
            ISolanaService solanasrv,
            IEncryptionService encryptionsrv,
            ILogger<WalletService> logger
        )
        {
            _cache = cache;
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _chainrepo = chainRepository;
            _walletrepo = walletrepo;
            _encryptionsrv = encryptionsrv;
            _solanasrv = solanasrv;
        }

        public async Task New(string id)
        {
            await Task.Run(async () =>
            {
                var chains = await _chainrepo.FindAll();
                foreach (var chain in chains)
                {
                    _logger.LogInformation($"creating wallet for chain: {chain.Name}");
                    string privateKey = "";
                    string publicKey = "";
                    switch (chain.Name)
                    {
                        case "solana":
                            _logger.LogInformation("creating solana wallet");
                            var wallet = _solanasrv.CreateWallet();
                            publicKey = wallet.Account.PublicKey.ToString();
                            privateKey = _encryptionsrv.Encrypt(wallet.Account.PrivateKey.ToString());
                            break;
                        case "xion":
                            break;
                        default:
                            throw new Exception("wallet not supported yet");
                    }

                    if (privateKey != "" && publicKey != "")
                    {
                        _logger.LogInformation($"adding public and private keys for {chain.Name} to wallet");
                        await _walletrepo.Create(new Wallet
                        {
                            UserId = id,
                            UUID = Guid.NewGuid(),
                            ChainId = chain.Id,
                            PublicKey = publicKey,
                            SecretKey = privateKey,
                            CreatedAt = DateTime.UtcNow,
                        });
                    }
                }
            });
        }

        public async Task<List<WalletResponse>> FindAllByUser(ClaimsPrincipal principal) {
            _logger.LogInformation("retrieve user information and cache response");
            string email = principal.FindFirstValue(ClaimTypes.Email)!;
            var user = await _cache.GetOrSetAsync(
                $"user:{email}",
                async _ => await _userManager.FindByEmailAsync(email),
                options => options.SetDuration(TimeSpan.FromMinutes(5)).SetFailSafe(true)
            );

            _logger.LogInformation("fetch wallet associated by UserID");
            var response = await _cache.GetOrSetAsync(
                $"wallet:{user?.Id}",
                async _ => await _walletrepo.FindAllByUserID(user!.Id),
                options => options.SetDuration(TimeSpan.FromMinutes(5)).SetFailSafe(true)
            );
            var wallets = response.Adapt<List<WalletResponse>>();
            return wallets;
        }

        public async Task<WalletResponse> FindByChainAndUser(ClaimsPrincipal principal, Guid chainID)
        {
            _logger.LogInformation("fetching authenticated user");
            string email = principal.FindFirstValue(ClaimTypes.Email)!;
            var user = await _cache.GetOrSetAsync(
                $"user:{email}",
                async _ => await _userManager.FindByEmailAsync(email),
                options => options.SetDuration(TimeSpan.FromMinutes(5)).SetFailSafe(true)
            );

            Console.WriteLine(await _walletrepo.FindByChainAndUserID(chainID, user!.Id));

            _logger.LogInformation("fetch wallet associated by UserID");
            var response = await _cache.GetOrSetAsync(
                $"wallet:{user?.Id}:{chainID}",
                async _ => await _walletrepo.FindByChainAndUserID(chainID, user!.Id),
                options => options.SetDuration(TimeSpan.FromMinutes(5)).SetFailSafe(true)
            );
            var wallets = response.Adapt<WalletResponse>();
            return wallets;
        }

        public async Task<ulong> FetchSolanaBalance(string publicKey)
        {
            var balance = await _solanasrv.GetBalance(publicKey);
            return balance;
        }
    }
}