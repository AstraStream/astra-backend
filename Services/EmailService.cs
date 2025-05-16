using FluentEmail.Core;
using Astra.Dtos.Entities;
using Astra.Services.Interfaces;

namespace Astra.Services {
    public class EmailService: IEmailService {
        private readonly IFluentEmail _email;
        public EmailService(
            IFluentEmail email
        ) {
            _email = email;
        }

        public async Task Send(EmailData data) {
            await _email.To(data.EmailAddress)
                .Subject(data.Subject)
                .Body(data.Body)
                .SendAsync();
        }
    }
}