using Astra.Dtos.Entities;
using System.Security.Claims;
using Astra.Dtos.Responses.Users;

namespace Astra.Services.Interfaces {
    public interface IAccountService {
        public Task<UserResponse> GetAuthUser(ClaimsPrincipal principal);
        public Task<(ServiceResult, byte[]?)> Setup2FA(ClaimsPrincipal principal);
    }
}