using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SpinBladeArena.Hubs;
using SpinBladeArena.LogicCenter;

namespace SpinBladeArena
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            ConfigureServices(builder.Services, builder.Configuration);

            WebApplication app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapHub<GameHub>("/gamehub");
            app.MapRazorPages();

            app.Run();
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            string? authKey = configuration["Key"] ?? throw new Exception("Please provide a key in the configuration");

            // Add services to the container.
            services.AddHttpContextAccessor();
            services.AddScoped<CurrentUser>();
            services.AddRazorPages();
            services.AddSingleton<GameManager>();
            services.AddSingleton<UserManager>();
            TokenValidationParameters tvp = new()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = nameof(SpinBladeArena),
                ValidAudience = nameof(SpinBladeArena),
                NameClaimType = "jti",
                IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(authKey)),
            };
            services.AddSingleton(tvp);
            services.AddSignalR();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = tvp;
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            PathString path = context.HttpContext.Request.Path;
                            if (path.StartsWithSegments("/gamehub"))
                            {
                                string? accessToken = context.Request.Query["access_token"];
                                if (!string.IsNullOrEmpty(accessToken))
                                {
                                    context.Token = accessToken;
                                }
                            }
                            return Task.CompletedTask;
                        }
                    };
                });
        }
    }
}
