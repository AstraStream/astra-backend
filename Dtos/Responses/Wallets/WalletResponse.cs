namespace Astra.Dtos.Responses.Wallets {
    public sealed record WalletResponse {
        public string Chain { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}