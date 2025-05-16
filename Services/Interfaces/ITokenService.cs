using Astra.Models;

namespace Astra.Services.Interfaces {
    public interface ITokenService {
        public string CreateAccessToken(User user, IList<string> roles);
        public string CreateRefreshToken(string email);
    }
}