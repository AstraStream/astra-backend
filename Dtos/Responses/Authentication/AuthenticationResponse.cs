namespace Astra.Dtos.Responses.Authentication {
    public sealed record AuthenticationResponse {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public bool? IsTwoFactorEnabled { get; set; }
        public string? Description { get; set; }
    } 
}