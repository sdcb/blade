using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SpinBladeArena.Hubs;
using SpinBladeArena.LogicCenter;
using SpinBladeArena.Performance;
using SpinBladeArena.Users;
using System.Security.Cryptography;

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

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                Native.timeBeginPeriod(1);
                app.Lifetime.ApplicationStopping.Register(() => Native.timeEndPeriod(1));
            }

            app.Run();
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            string? authKey = configuration["Key"];
            if (string.IsNullOrEmpty(authKey))
            {
                authKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
                Console.WriteLine($"Auth key not found in configuration, use generated: {authKey}");
            }

            // Add services to the container.
            services.AddHttpContextAccessor();
            services.AddScoped<CurrentUser>();
            services.AddRazorPages()
#if DEBUG
                .AddRazorRuntimeCompilation()
#endif
                ;
            services.AddSingleton<GameManager>();
            services.AddSingleton<UserManager>();
            services.AddKeyedSingleton(typeof(int), "ServerFPS", int.Parse(configuration["ServerFPS"] ?? "45"));
            services.AddKeyedSingleton(typeof(int), "AIPlayerCount", int.Parse(configuration["AIPlayerCount"] ?? "8"));
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
            services.AddSignalR()
                .AddMessagePackProtocol();
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
