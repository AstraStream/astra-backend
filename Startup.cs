using Astra.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public class Startup {
        public IConfiguration _configuration { get; }
        public IHostBuilder _hosting { get; }

        public Startup(IConfiguration configuration, IHostBuilder hosting) {
            _configuration = configuration;
            _hosting = hosting;
        }

        public void ConfigureServices(IServiceCollection services) {
            services
                .ConfigureDatabase(_configuration)
                .ConfigureIdentity()
                .ConfigureRatelimiting()
                .ConfigureSerilog(_hosting)
                .ConfigureJwt(_configuration)
                .ConfigureCors(_configuration)
                .ConfigureCache(_configuration)
                .ConfigureServices(_configuration)
                .ConfigureFluentEmail(_configuration);
        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env) {
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.RoutePrefix = string.Empty;
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "AstraAPI V1");
                });
            }
            
            app.UseCors("CorsPolicy");
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
        }
    }