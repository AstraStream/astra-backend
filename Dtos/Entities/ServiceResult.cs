using System.Net;
using Microsoft.AspNetCore.Identity;

namespace Astra.Dtos.Entities {
    public sealed record ServiceResult {
        public HttpStatusCode Code { get; set; }
        public required IdentityResult IdentityResult { get; set; }
    }
}