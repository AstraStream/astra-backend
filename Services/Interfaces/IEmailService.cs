using Astra.Dtos.Entities;

namespace Astra.Services.Interfaces {
    public interface IEmailService {
        public Task Send(EmailData data);
    }
}