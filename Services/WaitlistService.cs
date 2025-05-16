using Mapster;
using System.Net;
using Astra.Models;
using Astra.Repositories;
using Astra.Dtos.Entities;
using Astra.Dtos.Responses;
using Astra.Services.Interfaces;
using Astra.Dtos.Queries.Waitlists;
using Astra.Dtos.Requests.Waitlists;
using Astra.Repositories.Interfaces;
using ZiggyCreatures.Caching.Fusion;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Astra.Services {
    public class WaitlistService: IWaitlistService {
        private readonly IFusionCache _cache;
        private readonly ILogger<WaitlistService> _logger;
        private readonly IWaitlistRepository _waitlistrepo;

        public WaitlistService(
            IFusionCache cache,
            ILogger<WaitlistService> logger,
            IWaitlistRepository waitlistrepo
        ) {
            _cache = cache;
            _logger = logger;
            _waitlistrepo = waitlistrepo;
        }

        public async Task<PaginatedResult<Waitlist>> FindAll(WaitlistQuery query) {
            _logger.LogInformation("fetching wailists data from database and caching");
            var waitlists = await _cache.GetOrSetAsync(
                "waitlist:all",
                 async _ => {
                    _logger.LogInformation("retrieve waitlists querable response");
                    return await _waitlistrepo.FindAll(query);
                },
                options => options.SetDuration(TimeSpan.FromSeconds(20)).SetFailSafe(true)
            );
            return waitlists;
        }

        public async Task<ServiceResult> Create(StoreWaitlistRequest request) {
           try {  
                _logger.LogInformation("check if email is in waitlist");
                if (await _waitlistrepo.Exists(request.Email)) {
                    _logger.LogError("email address already registered in waitlist");
                    return (
                        new ServiceResult {
                            Code = HttpStatusCode.Conflict,
                            IdentityResult = IdentityResult.Failed(new IdentityError{
                                Code = "DuplicateWaitlistEntry",
                                Description = "email address already registered in waitlist"
                            })
                        }
                    );
                }

                _logger.LogInformation("storing waitlist into database");
                var waitlist = request.Adapt<Waitlist>();
                waitlist.UUID = Guid.NewGuid();
                await _waitlistrepo.Create(waitlist);
    
                return (
                    new ServiceResult {
                        Code = HttpStatusCode.OK,
                        IdentityResult = IdentityResult.Success,
                    }
                );

            } catch(Exception e) {
                _logger.LogError($"encountered an error storing waitlist {e}");

                return (
                    new ServiceResult {
                        Code = HttpStatusCode.InternalServerError,
                        IdentityResult = IdentityResult.Failed(new IdentityError{
                            Code = "CreateWaitlistError",
                            Description = "could not store waitlist data"
                        }),
                    }
                );
            }
        }

    }
}