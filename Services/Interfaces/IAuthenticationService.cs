using Astra.Dtos.Entities;
using System.Security.Principal;
using Astra.Dtos.Requests.Authentication;
using Astra.Dtos.Responses.Authentication;

namespace Astra.Services.Interfaces {
 public interface IAuthenticationService
 {  
  public Task Logout(IIdentity identity);

  public Task<ServiceResult> RegisterAccount(RegisterRequest request);

  public Task<(ServiceResult, AuthenticationResponse?)> CompleteRegistration(CompleteRegistrationRequest request);

  public Task<ServiceResult> ForgotPassword(ForgotPasswordRequest request);

  public Task<ServiceResult> VerifyConfirmationEmail(VerifyEmailRequest request);

  public Task<ServiceResult> SendConfirmationEmail(SendEmailConfirmationRequest request);

  public Task<ServiceResult> ResetPassword(ResetPasswordRequest request);

  public Task<(ServiceResult, AuthenticationResponse?)> LoginAccount(LoginRequest request);
 }
}