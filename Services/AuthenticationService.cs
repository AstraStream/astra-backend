using Mapster;
using System.Net;
using Astra.Models;
using FluentEmail.Core;
using Astra.Dtos.Entities;
using Astra.Services.Interfaces;
using System.Security.Principal;
using Microsoft.AspNetCore.Identity;
using ZiggyCreatures.Caching.Fusion;
using Astra.Dtos.Requests.Authentication;
using Astra.Dtos.Responses.Authentication;

namespace Astra.Services {
    public class AuthenticationService: IAuthenticationService {

        private readonly IEmailService _emailsrv;
        private readonly IFusionCache _cache;
        private readonly ITokenService _tokensrv;
        private readonly IRewardService _rewardsrv;
        private readonly IWalletService _walletsrv;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(
            IFusionCache cache,
            IEmailService emailsrv,
            ITokenService tokensrv,
            IWalletService walletsrv,
            IRewardService rewardsrv,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<AuthenticationService> logger
        ) {
            _cache = cache;
            _logger = logger;
            _emailsrv = emailsrv;
            _tokensrv = tokensrv;
            _rewardsrv = rewardsrv;
            _walletsrv = walletsrv;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<ServiceResult> RegisterAccount(RegisterRequest request) {
            // Check if main exists already
            _logger.LogInformation($"Check if email address {request.Email} exists");
            if (await _userManager.FindByEmailAsync(request.Email) != null) {
                _logger.LogError($"Email address {request.Email} already taken");
                return (
                    new ServiceResult 
                    {
                        Code = HttpStatusCode.Conflict,
                        IdentityResult = IdentityResult.Failed(_userManager.ErrorDescriber.DuplicateEmail(request.Email)),
                    }
                );
            }

            // Add user to database
            var user = request.Adapt<User>();
            user.UserName = request.Email.Split("@")[0];
            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                _logger.LogError($"{string.Join(",", result.Errors.Select(e => e.Description))}");
                return (
                    new ServiceResult
                    {
                        Code = HttpStatusCode.InternalServerError,
                        IdentityResult = IdentityResult.Failed(result.Errors.ToArray()),
                    }
                );
            }

            try {
                // send confirmation code
                var emailVerificationCode = await _userManager.GenerateEmailConfirmationTokenAsync(user);
               
                await _emailsrv.Send(new EmailData {
                    EmailAddress = user.Email!,
                    Subject = "Registration Complete | Confirm Email",
                    Body = $"Verification code {emailVerificationCode}",
                });

            } catch(Exception e) {
                // delete newly created account
                _logger.LogError("Deleting user's account");
                await _userManager.DeleteAsync(user);

                _logger.LogError(e.ToString());
                return (
                    new ServiceResult
                    {
                        Code = HttpStatusCode.BadRequest,
                        IdentityResult = IdentityResult.Failed(new IdentityError { 
                            Code = "UnexpectedError",
                            Description = "could not send mail to user" 
                        })
                    }
                );
            }

            // Return response
             _logger.LogInformation("user registered successfully");
            return (
                new ServiceResult
                {
                    Code = HttpStatusCode.OK,
                    IdentityResult = IdentityResult.Success,
                }
            );
        }

        public async Task<(ServiceResult, AuthenticationResponse?)> CompleteRegistration(CompleteRegistrationRequest request) {
            _logger.LogInformation("find account with email address");;
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                _logger.LogError("no account associated with this email");
                return (
                    new ServiceResult
                    {
                        Code = HttpStatusCode.NotFound,
                        IdentityResult = IdentityResult.Failed(new IdentityError { 
                            Code = "EmailNotFound",
                            Description = "no account associated with this email"
                        })
                    }, null
                );
            }

            if (await _userManager.IsEmailConfirmedAsync(user) == false) {
                return (
                    new ServiceResult {
                        Code = HttpStatusCode.Unauthorized,
                        IdentityResult = IdentityResult.Failed(new IdentityError {
                            Code = "EmailNotVerified",
                            Description = "email address not verified"
                        })
                    }, null
                );
            }

            // update users data
            _logger.LogInformation("updating user data");
            user.Gender = request.Gender;
            user.Country = request.Country;
            user.UserName = request.UserName;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) {
                _logger.LogError("there was an error updating user's data");
                return (
                    new ServiceResult {
                        Code = HttpStatusCode.InternalServerError,
                        IdentityResult = IdentityResult.Failed(new IdentityError {
                            Code = "AccountCreationError",
                            Description = "could not insert user data"
                        })
                    }, null
                );
            }
            
            // Create user wallets
            try {
                _logger.LogInformation("create wallets for user");
                await _walletsrv.New(user.Id);
            } catch (Exception e) {
                _logger.LogError($"could not create wallet {e}");
                return (
                    new ServiceResult
                    {
                        Code = HttpStatusCode.InternalServerError,
                        IdentityResult = IdentityResult.Failed(new IdentityError {
                            Code = "WalletCreationError",
                            Description = "could not create wallet for user"
                        }),
                    }, null
                );
            }

            // Assign role to user
             var roleResult = await _userManager.AddToRoleAsync(user, "Streamer");
            if (!roleResult.Succeeded)
            {
                _logger.LogError($"could not assign role to user");
                return (
                    new ServiceResult
                    {
                        Code = HttpStatusCode.InternalServerError,
                        IdentityResult = IdentityResult.Failed(roleResult.Errors.ToArray()),
                    }, null
                );
            }

            // reward user
            try {
                _logger.LogInformation("Rewarding new user");
                await _rewardsrv.Create(user.Id);
            } catch (Exception e) {
                _logger.LogError($"encountered an rewarding user {e}");
                return (
                    new ServiceResult {
                        Code = HttpStatusCode.InternalServerError,
                        IdentityResult = IdentityResult.Failed(new IdentityError{
                            Code = "OnboardRewardError",
                            Description = "could not reward user"
                        }),
                    }, null
                );
            }

            // get user roles
            var roles = await _userManager.GetRolesAsync(user);

            return (
                new ServiceResult
                {
                    Code = HttpStatusCode.OK,
                    IdentityResult = IdentityResult.Success,
                },
                new AuthenticationResponse
                {
                    AccessToken = _tokensrv.CreateAccessToken(user, roles),
                    RefreshToken = _tokensrv.CreateRefreshToken(user.Email!)
                }
            );
        }
        
        public async Task<(ServiceResult, AuthenticationResponse?)> LoginAccount(LoginRequest request) {
            try {
                _logger.LogInformation("find account with email address");
                var user = await _cache.GetOrSetAsync(
                    $"user:{request.Email}",
                    async _ => await _userManager.FindByEmailAsync(request.Email),
                    options => options.SetDuration(TimeSpan.FromMinutes(1)).SetFailSafe(true)
                );
                if (user is null)
                {
                    _logger.LogError("no account associated with this email");
                    return (
                        new ServiceResult
                        {
                            Code = HttpStatusCode.NotFound,
                            IdentityResult = IdentityResult.Failed(new IdentityError { 
                                Code = "EmailNotFound",
                                Description = "no account associated with this email"
                            })
                        }, null
                    );
                }

                if (await _userManager.IsEmailConfirmedAsync(user) == false) {
                    return (
                        new ServiceResult {
                            Code = HttpStatusCode.Unauthorized,
                            IdentityResult = IdentityResult.Failed(new IdentityError {
                                Code = "EmailNotVerified",
                                Description = "email address not verified"
                            })
                        }, null
                    );
                }

                if (!await _userManager.CheckPasswordAsync(user, request.Password))
                {
                    _logger.LogError("provided password is incorrect");
                    return (
                        new ServiceResult
                        {
                            Code = HttpStatusCode.BadRequest,
                            IdentityResult = IdentityResult.Failed(new IdentityError { 
                                Code = "PasswordIncorrect",
                                Description = "provided password is incorrect" 
                            })
                        }, null
                    );
                }

                if (user.TwoFactorEnabled)
                {
                    // send two factor code to users' email
                    // _userManager.

                    _logger.LogInformation($"two factor enabled for account {user.Email}");
                    return (
                        new ServiceResult
                        {
                            Code = HttpStatusCode.OK,
                            IdentityResult = IdentityResult.Success,
                        },
                        new AuthenticationResponse
                        {
                            IsTwoFactorEnabled = true,
                            Description = "Two factor enabled for account"
                        }
                    );
                }

                // get user role
                var roles = await _userManager.GetRolesAsync(user);

                // send login session
                await _emailsrv.Send(new EmailData {
                    EmailAddress = user.Email!,
                    Subject = "Login Session",
                    Body = $"Login access to account {user.Email}"
                });

                _logger.LogInformation("user authenticated successfully");
                return (
                    new ServiceResult
                    {
                        Code = HttpStatusCode.OK,
                        IdentityResult = IdentityResult.Success,
                    },
                    new AuthenticationResponse
                    {
                        AccessToken = _tokensrv.CreateAccessToken(user, roles),
                        RefreshToken = _tokensrv.CreateRefreshToken(user.Email!)
                    }
                );
            } catch(Exception e) {
                _logger.LogError(e.ToString());
                return (
                    new ServiceResult
                    {
                        Code = HttpStatusCode.BadRequest,
                        IdentityResult = IdentityResult.Failed(new IdentityError { 
                            Code = "UnexpectedError",
                            Description = "could not send mail to user" 
                        })
                    }, null
                );
            }
        }

        public async Task<ServiceResult> SendConfirmationEmail(SendEmailConfirmationRequest request) {
            // Check if main exists already
            _logger.LogInformation($"Check if email address {request.Email} exists");
            var user = await _cache.GetOrSetAsync(
                $"user:{request.Email}",
                async _ => await _userManager.FindByEmailAsync(request.Email),
                options => options.SetDuration(TimeSpan.FromMinutes(1)).SetFailSafe(true)
            );

            if (user is null) {
                _logger.LogError($"Email address {request.Email} not found or registered");
                return (
                    new ServiceResult 
                    {
                        Code = HttpStatusCode.NotFound,
                        IdentityResult = IdentityResult.Failed(new IdentityError {
                            Code = "EmailNotFoundOrRegistered",
                            Description = "email address not found or registered"
                        }),
                    }
                );
            }

            // check if email has already been verified
            _logger.LogInformation($"Check if email address {user.Email} is verified");
             if (await _userManager.IsEmailConfirmedAsync(user) == true) {
                _logger.LogError($"Email address {user.Email} already verified");
                return (
                    new ServiceResult {
                        Code = HttpStatusCode.BadRequest,
                        IdentityResult = IdentityResult.Failed(new IdentityError {
                            Code = "EmailAlreadyVerified",
                            Description = "email address already verified"
                        })
                    }
                );
            }

            try {
                 // send confirmation code
                _logger.LogInformation($"Sending email confirmation code to {request.Email}");
                var emailVerificationCode = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                
                await _emailsrv.Send(new EmailData {
                    EmailAddress = user.Email!,
                    Subject = "Confirmation Email Action",
                    Body = $"Verification code {emailVerificationCode}"
                });

            } catch(Exception e) {
                _logger.LogError(e.ToString());
                return (
                    new ServiceResult
                    {
                        Code = HttpStatusCode.BadRequest,
                        IdentityResult = IdentityResult.Failed(new IdentityError { 
                            Code = "UnexpectedError",
                            Description = "could not send mail to user" 
                        })
                    }
                );
            }

            _logger.LogInformation($"Verification code sent to {request.Email} successfully");
            return (
                new ServiceResult {
                    Code = HttpStatusCode.OK,
                    IdentityResult = IdentityResult.Success,
                }
            );
        }

        public async Task<ServiceResult> VerifyConfirmationEmail(VerifyEmailRequest request) {
            _logger.LogInformation("find account with email address");
            var user = await _cache.GetOrSetAsync(
                $"user:{request.Email}",
                async _ => await _userManager.FindByEmailAsync(request.Email),
                options => options.SetDuration(TimeSpan.FromMinutes(1)).SetFailSafe(true)
            );
            if (user is null) {
                _logger.LogError($"Email address {request.Email} not found or registered");
                return (
                    new ServiceResult 
                    {
                        Code = HttpStatusCode.NotFound,
                        IdentityResult = IdentityResult.Failed(new IdentityError {
                            Code = "EmailNotFoundOrRegistered",
                            Description = "email address not found or registered"
                        }),
                    }
                );
            }

            // verify email address
            _logger.LogInformation($"Verifying email address {user.Email}");
            var emailVerificationResult = await _userManager.ConfirmEmailAsync(user, request.Code);
            if (!emailVerificationResult.Succeeded) {
                _logger.LogError($"Email verification for {user.Email} was not successfull");
                return (
                    new ServiceResult {
                        Code = HttpStatusCode.BadRequest,
                        IdentityResult = IdentityResult.Failed(emailVerificationResult.Errors.ToArray())
                    }
                );
            }

            return (
                new ServiceResult {
                    Code = HttpStatusCode.OK,
                    IdentityResult = IdentityResult.Success
                }
            );
        }

        public async Task<ServiceResult> ForgotPassword(ForgotPasswordRequest request) {
            // check if provided email address exists
            _logger.LogInformation("find account with email address");
            var user = await _cache.GetOrSetAsync(
                $"user:{request.Email}",
                async _ => await _userManager.FindByEmailAsync(request.Email),
                options => options.SetDuration(TimeSpan.FromMinutes(1)).SetFailSafe(true)
            );
            if (user is null) {
                _logger.LogError($"Email address {request.Email} not found or registered");
                return (
                    new ServiceResult 
                    {
                        Code = HttpStatusCode.NotFound,
                        IdentityResult = IdentityResult.Failed(new IdentityError {
                            Code = "EmailNotFoundOrRegistered",
                            Description = "email address not found or registered"
                        }),
                    }
                );
            }

            try {
                // generate reset token and send email
                _logger.LogInformation($"Generating password reset token for {user.Email}");
                var passwordResetCode = await _userManager.GeneratePasswordResetTokenAsync(user);
                await _emailsrv.Send(new EmailData {
                    EmailAddress = user.Email!,
                    Subject = "Reset Password Action",
                    Body = $"Reset code {passwordResetCode}"
                });
            } catch(Exception e) {
                _logger.LogError(e.ToString());
                return (
                    new ServiceResult
                    {
                        Code = HttpStatusCode.BadRequest,
                        IdentityResult = IdentityResult.Failed(new IdentityError { 
                            Code = "UnexpectedError",
                            Description = "could not send mail to user" 
                        })
                    }
                );
            }

            return (
                new ServiceResult {
                    Code = HttpStatusCode.OK,
                    IdentityResult = IdentityResult.Success
                }
            );
        }

        public async Task<ServiceResult> ResetPassword(ResetPasswordRequest request) {
            // check if provided email address exists
            _logger.LogInformation("find account with email address");
            var user = await _cache.GetOrSetAsync(
                $"user:{request.Email}",
                async _ => await _userManager.FindByEmailAsync(request.Email),
                options => options.SetDuration(TimeSpan.FromMinutes(1)).SetFailSafe(true)
            );
            if (user is null) {
                _logger.LogError($"Email address {request.Email} not found or registered");
                return (
                    new ServiceResult 
                    {
                        Code = HttpStatusCode.NotFound,
                        IdentityResult = IdentityResult.Failed(new IdentityError {
                            Code = "EmailNotFoundOrRegistered",
                            Description = "email address not found or registered"
                        })
                    }
                );
            }

            // check if password exists
            _logger.LogInformation("check if password and password confirmation matches");
            if (!request.Password.Equals(request.PasswordConfirmation)) {
                _logger.LogError("password does not match");
                return (
                    new ServiceResult {
                        Code = HttpStatusCode.BadRequest,
                        IdentityResult = IdentityResult.Failed(new IdentityError {
                            Code = "PasswordDoesNotMatch",
                            Description = "password does not match"
                        })
                    }
                );
            }

            // reset password
            _logger.LogInformation($"Attempting to reset password for user {request.Email}");
            var resetPasswordResult = await _userManager.ResetPasswordAsync(user, request.Code, request.Password);
            if (!resetPasswordResult.Succeeded) {
                _logger.LogError($"{string.Join(",", resetPasswordResult.Errors.Select(e => e.Description))}");
                return (
                    new ServiceResult {
                        Code = HttpStatusCode.InternalServerError,
                        IdentityResult = IdentityResult.Failed(resetPasswordResult.Errors.ToArray())
                    }
                );
            }

            _logger.LogInformation("updated password successfully");
            return (
                new ServiceResult {
                    Code = HttpStatusCode.OK,
                    IdentityResult = IdentityResult.Success
                }
            );
        }

        public async Task Logout(IIdentity identity) {
            _logger.LogInformation($"logging {identity.Name} out");
            await _signInManager.SignOutAsync();
        }
    }
}