namespace Astra.Dtos.Entities {
    public sealed record EmailData {
        public string EmailAddress { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }
}