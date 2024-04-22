using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
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
            TokenValidationParameters tvp = new()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = nameof(SpinBladeArena),
                ValidAudience = nameof(SpinBladeArena),
                NameClaimType = "jti",
                IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(authKey))
            };
            services.AddSingleton(tvp);
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = tvp;
                });
        }
    }
}
