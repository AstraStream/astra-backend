using Mapster;
using Serilog;
using Solnet.Rpc;
using Astra.Models;
using Astra.Common;
using Astra.Database;
using Astra.Services;
using System.Reflection;
using Astra.Repositories;
using Astra.Services.Http;
using Astra.Configurations;
using Astra.Dtos.Responses;
using Astra.Database.Seeders;
using Microsoft.OpenApi.Models;
using Astra.Services.Interfaces;
using Astra.Repositories.Interfaces;
using ZiggyCreatures.Caching.Fusion;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

namespace Astra.Extensions
{
    public static class ServiceExtensions
    {
        // configure database services
        public static IServiceCollection ConfigureDatabase(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<AstraDbContext>(options =>
            {
                options.UseNpgsql(config.GetConnectionString("DefaultConnection"));
            });
            return services;
        }

        // configure identity services
        public static IServiceCollection ConfigureIdentity(this IServiceCollection services)
        {
            services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 12;

                options.User.RequireUniqueEmail = true;

                options.SignIn.RequireConfirmedEmail = true;

                options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
                options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
            })
            .AddEntityFrameworkStores<AstraDbContext>()
            .AddDefaultTokenProviders();
            return services;
        }

        // configure jwt
        public static IServiceCollection ConfigureJwt(this IServiceCollection services, IConfiguration config)
        {
            var key = System.Text.Encoding.UTF8.GetBytes(config["Jwt:SigningKey"]!);
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = config["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = config["Jwt:Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });
            services.AddAuthorization();
            return services;
        }

        // configure cors
        public static IServiceCollection ConfigureCors(this IServiceCollection services, IConfiguration config)
        {
            var origins = config["Cors:AllowedOrigins"]!.Split(",") ?? new String[] { };
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder.WithOrigins(origins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });
            return services;
        }

        // configure services
        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddOpenApi();
            services.AddMapster();
            services.AddControllers().AddNewtonsoftJson(options => {
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });
            services.AddSwaggerGen(options => {
                options.SwaggerDoc("v1", new OpenApiInfo {
                    Title = "Astra",
                    Version = "v1",
                    Description = "Astra Web3 Streaming Platform API"
                });

                var jwtSecurityScheme = new OpenApiSecurityScheme {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    Reference = new OpenApiReference {
                        Type = ReferenceType.SecurityScheme,
                        Id = JwtBearerDefaults.AuthenticationScheme,
                    }
                };

                options.AddSecurityDefinition("Bearer", jwtSecurityScheme);

                options.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    { jwtSecurityScheme, new List<string>() }
                });

                // options.OperationFilter<SecurityRequirementsOperationFilter>();
            });

            // configure mapster
            MapsterConfiguration.ConfigureMapster();

            // configure pagination
            services.AddScoped(typeof(Pagination<>));

            // Add Repositories
            services.AddScoped<IChainRepository, ChainRepository>();
            services.AddScoped<IRewardRepository, RewardRepository>();
            services.AddScoped<IWaitlistRepository, WaitlistRepository>();
            services.AddScoped<IWalletRepository, WalletRepository>();

            // Add Services
            services.AddScoped<AuthUserHelper>();
            services.AddTransient<DatabaseSeeder>();
            services.AddScoped<IChainService, ChainService>();
            services.AddScoped<IRewardService, RewardService>();
            services.AddScoped<IWalletService, WalletService>();
            services.AddScoped<ISolanaService, SolanaService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddTransient<ITokenService, TokenService>();
            services.AddTransient<IEmailService, EmailService>();
            services.AddScoped<IWaitlistService, WaitlistService>();
            services.AddScoped<IEncryptionService, EncryptionService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();

            // add http clients
            services.AddHttpClient<XionClient>();
            
            services.AddSingleton<IRpcClient>(sp =>
                ClientFactory.GetClient(config.GetValue<string>("Chains:Solana:RPC_ENDPOINT"))
            );

            return services;
        }

        public static IServiceCollection ConfigureCache(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure fusion cache.
            services.AddFusionCache()
                .WithDefaultEntryOptions(option => option.Duration = TimeSpan.FromSeconds(20))
                .WithSerializer(new FusionCacheSystemTextJsonSerializer())
                .WithDistributedCache(
                    new RedisCache(new RedisCacheOptions
                    {
                        Configuration = configuration.GetConnectionString("Redis")
                    })
                );
            return services;
        }

        // configure fluent email
        public static IServiceCollection ConfigureFluentEmail(this IServiceCollection services, IConfiguration config) {
            services
                .AddFluentEmail(config["Email:From"], "Dann")
                .AddSmtpSender(config["Email:Host"], config.GetValue<int>("Email:Port"));
                // .AddSmtpSender(config["Email:Host"], 1025, config["Email:Username"], config["Email:Password"]);

            return services;
        }

        // configure ratelimiting
        public static IServiceCollection ConfigureRatelimiting(this IServiceCollection services)
        {
            // Configure built-in rate limiting middleware.
            // Limits to 10 requests per minute
            services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 10,
                            Window = TimeSpan.FromMinutes(1),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        }
                    )
                );
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            });
            return services;
        }

        // configure serilog
        public static IServiceCollection ConfigureSerilog(this IServiceCollection services, IHostBuilder host)
        {
            // Configure Serilog for logging
            host.UseSerilog((context, services, configuration) => configuration.WriteTo.Console());
            return services;
        }

    }
}