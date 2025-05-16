using Mapster;
using System.Net;
using Astra.Models;
using Astra.Dtos.Entities;
using Astra.Dtos.Responses;
using Astra.Services.Interfaces;
using Astra.Dtos.Queries.Chains;
using Astra.Dtos.Requests.Chains;
using Astra.Dtos.Responses.Chains;
using Microsoft.AspNetCore.Identity;
using Astra.Repositories.Interfaces;
using ZiggyCreatures.Caching.Fusion;
using Microsoft.EntityFrameworkCore;

namespace Astra.Services {
    public class ChainService: IChainService {

        private readonly ILogger<ChainService> _logger;
        private readonly IChainRepository _chainrepo;
        private readonly IFusionCache _cache;

        public ChainService(
            ILogger<ChainService> logger, 
            IChainRepository chainrepo,
            IFusionCache cache
        ) {
                _cache = cache;
                _logger = logger;
                _chainrepo = chainrepo;
        }

        public async Task<ServiceResult> Create(StoreChainRequest request) {
            try {  
                _logger.LogInformation("check if chain exists");
                if (await _chainrepo.Exists(request.Name)) {
                    _logger.LogError("chain already exists");
                    return (
                        new ServiceResult {
                            Code = HttpStatusCode.Conflict,
                            IdentityResult = IdentityResult.Failed(new IdentityError{
                                Code = "DuplicateChainEntry",
                                Description = "chain already exists"
                            })
                        }
                    );
                }

                _logger.LogInformation("storing chain into database");
                var chain = request.Adapt<Chain>();
                await _chainrepo.Create(chain);
    
                return (
                    new ServiceResult {
                        Code = HttpStatusCode.OK,
                        IdentityResult = IdentityResult.Success,
                    }
                );

            } catch(Exception e) {
                _logger.LogError($"encountered an error storing chain {e}");

                return (
                    new ServiceResult {
                        Code = HttpStatusCode.InternalServerError,
                        IdentityResult = IdentityResult.Failed(new IdentityError{
                            Code = "CreateChainError",
                            Description = "could not store chain data"
                        }),
                    }
                );
            }
        }

        public async Task<List<ChainResponse>> FindAll() {
            _logger.LogInformation("Fetching all chains");
            var chains = await _cache.GetOrSetAsync(
                $"chains:all",
                async _ => await _chainrepo.FindAll(),
                options => options.SetDuration(TimeSpan.FromSeconds(30)).SetFailSafe(true)
            );

            return chains.Adapt<List<ChainResponse>>();
        }

        public async Task<PaginatedResult<ChainResponse>> FindAllPaginated(ChainQuery query) {
            // cache chains response
            _logger.LogInformation("fetching chains data from database and caching");
            var result = await _cache.GetOrSetAsync(
                "chains_paginated:all",
                 async _ => {
                    _logger.LogInformation("retrieve chain querable response");
                    return await _chainrepo.FindAllPaginated(query);
                },
                options => options.SetDuration(TimeSpan.FromSeconds(20)).SetFailSafe(true)
            );

            _logger.LogInformation("map chains and return all");
            var chainResponses = result.Data.Adapt<List<ChainResponse>>();
            return new PaginatedResult<ChainResponse>
            {
                TotalCount = result.TotalCount,
                Data = chainResponses,
                NextLink = result.NextLink,
                PreviousLink = result.PreviousLink,
                HasNext = result.HasNext,
                HasPrevious = result.HasPrevious
            };
        }
    
        public async Task<(ServiceResult, ChainResponse?)> Find(Guid uuid) {
            _logger.LogInformation("retrieving chain data from database and caching");
            var response = await _cache.GetOrSetAsync(
                $"chains:{uuid}",
                async _ => await _chainrepo.Find(uuid),
                options => options.SetDuration(TimeSpan.FromSeconds(30)).SetFailSafe(true)
            );

            if (response == null) {
                _logger.LogError("chain record not found");
                return (
                    new ServiceResult {
                        Code = HttpStatusCode.NotFound,
                        IdentityResult = IdentityResult.Failed(new IdentityError{
                            Code = "ChainDataNotFound",
                            Description = "chain record not found"
                        })
                    }, null
                );
            }

            _logger.LogInformation("map chain to chain response and return data");
            var chain = response.Adapt<ChainResponse>();
            return (
                    new ServiceResult {
                        Code = HttpStatusCode.OK,
                        IdentityResult = IdentityResult.Success
                    }, chain
                );
        }

        public async Task<ServiceResult> Update(Guid uuid, UpdateChainRequest request) {
            // check if chain exists
            _logger.LogInformation("retrieve chain from database if it exists");
            var chain = await _cache.GetOrSetAsync(
                $"chains:{uuid}",
                async _ => await _chainrepo.Find(uuid),
                options => options.SetDuration(TimeSpan.FromSeconds(40)).SetFailSafe(true)
            );

            if (chain == null) {
                _logger.LogError("chain record not found in database");
                return new ServiceResult {
                    Code = HttpStatusCode.NotFound,
                    IdentityResult = IdentityResult.Failed(new IdentityError {
                        Code = "ChainNotFound",
                        Description = "chain record not found"
                    })
                };
            }

            try {
                // update necessary column
                _logger.LogInformation("updating chain record");
                await _chainrepo.Update(chain, request.Name);

                return new ServiceResult {
                    Code = HttpStatusCode.OK,
                    IdentityResult = IdentityResult.Success
                };

            } catch (Exception e) {
                _logger.LogError($"could not update chain record {e}");
                return new ServiceResult {
                    Code = HttpStatusCode.OK,
                    IdentityResult = IdentityResult.Failed(new IdentityError {
                        Code = "ChainUpdateError",
                        Description = "encountered an error updating chain"
                    })
                };
            }
        }
    
        public async Task<ServiceResult> Delete(Guid uuid) {
            // check if chain exists
            _logger.LogInformation("retrieve chain from database if it exists");
            var chain = await _cache.GetOrSetAsync(
                $"chains:{uuid}",
                async _ => await _chainrepo.Find(uuid),
                options => options.SetDuration(TimeSpan.FromSeconds(40)).SetFailSafe(true)
            );

            if (chain == null) {
                _logger.LogError("chain record not found in database");
                return new ServiceResult {
                    Code = HttpStatusCode.NotFound,
                    IdentityResult = IdentityResult.Failed(new IdentityError {
                        Code = "ChainNotFound",
                        Description = "chain record not found"
                    })
                };
            }

            try {
                _logger.LogInformation($"deleting chain record {chain.UUID}");
                await _chainrepo.Delete(chain);
                
                return new ServiceResult {
                    Code = HttpStatusCode.OK,
                    IdentityResult = IdentityResult.Success
                };

            } catch (Exception e) {
                _logger.LogError($"could not delete chain record {e}");
                return new ServiceResult {
                    Code = HttpStatusCode.OK,
                    IdentityResult = IdentityResult.Failed(new IdentityError {
                        Code = "ChainDeleteError",
                        Description = "encountered an error deleting chain"
                    })
                };   
            }
        }
    }
}