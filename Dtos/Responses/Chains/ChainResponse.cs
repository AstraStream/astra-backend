namespace Astra.Dtos.Responses.Chains {
    public sealed record ChainResponse {
        public Guid UUID { get; set; }
        public string? Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}