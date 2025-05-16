namespace Astra.Dtos.Responses.Users {
    public sealed record UserResponse {
        public string? UserName { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public string? Country { get; set; } = string.Empty;
        public string? Gender { get; set; } = string.Empty;
        public bool TwoFactorEnabled { get; set; } 
        public bool OAuthTwoFactorEbabled { get; set; }
        public List<string> Roles {get; set;} = new();
    }
}