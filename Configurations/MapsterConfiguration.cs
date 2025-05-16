using Mapster;
using Astra.Models;
using Astra.Dtos.Requests.Chains;
using Astra.Dtos.Responses.Users;
using Astra.Dtos.Responses.Chains;
using Astra.Dtos.Responses.Wallets;
using Astra.Dtos.Requests.Waitlists;
using Astra.Dtos.Requests.Authentication;

namespace Astra.Configurations {
    public static class MapsterConfiguration {
        public static void ConfigureMapster() {
            TypeAdapterConfig<RegisterRequest, User>.NewConfig()
                .Map(dest => dest.Email, src => src.Email);

            TypeAdapterConfig<Chain, ChainResponse>.NewConfig()
                .Map(dest => dest.UUID, src => src.UUID)
                .Map(dest => dest.Name, src => src.Name)
                .Map(dest => dest.CreatedAt, src => src.CreatedAt);

            TypeAdapterConfig<StoreChainRequest, Chain>.NewConfig()
                .Map(dest => dest.Name, src => src.Name);

            TypeAdapterConfig<User, UserResponse>.NewConfig()
                .Map(dest => dest.Email , src => src.Email)
                .Map(dest => dest.Gender , src => src.Gender)
                .Map(dest => dest.Country , src => src.Country)
                .Map(dest => dest.UserName, src => src.UserName)
                .Map(dest => dest.TwoFactorEnabled, src => src.TwoFactorEnabled)
                .Map(dest => dest.OAuthTwoFactorEbabled, src => src.TotpTwoFactorEnabled);

            TypeAdapterConfig<StoreWaitlistRequest, Waitlist>.NewConfig()
                .Map(dest => dest.FullName, src => src.FullName)
                .Map(dest => dest.Email, src => src.Email);

            TypeAdapterConfig<Wallet, WalletResponse>.NewConfig()
                .Map(dest => dest.Chain, src => src.Chain!.UUID)
                .Map(dest => dest.Address, src => src.PublicKey);
        }
    }
}