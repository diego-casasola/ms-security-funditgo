using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Security.Application;
using Security.Infrastructure.EntityFramework;
using Security.Infrastructure.Security;
using System.Reflection;
using System.Text;

namespace Security.Infrastructure
{
    public static class Extensions
    {
        public static void AddSecurity(IServiceCollection services, IConfiguration configuration)
        {

            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                options.Lockout.MaxFailedAccessAttempts = 5;
            }).AddEntityFrameworkStores<SecurityDbContext>()
            .AddDefaultTokenProviders();

            services.AddAuthorization(config =>
            {
                var defaultAuthBuilder = new AuthorizationPolicyBuilder();
                var defaultAuthPolicy = defaultAuthBuilder
                    .RequireAuthenticatedUser()
                    .Build();

                config.DefaultPolicy = defaultAuthPolicy;

                foreach (var mnemonic in ApplicationPermission.GetAllPermissions().Select(x => x.Mnemonic))
                {
                    config.AddPolicy(mnemonic,
                        policy => policy.RequireClaim("Permission", new string[] { mnemonic }));
                }
            });

            JwtOptions jwtoptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();

            services.AddAuthentication().AddJwtBearer("Bearer", jwtOptions =>
            {
                var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtoptions.SecretKey));
                jwtOptions.TokenValidationParameters = new TokenValidationParameters()
                {
                    IssuerSigningKey = signingKey,
                    ValidateIssuer = jwtoptions.ValidateIssuer,
                    ValidateAudience = jwtoptions.ValidateAudience,
                    ValidIssuer = jwtoptions.ValidIssuer,
                    ValidAudience = jwtoptions.ValidAudience
                };
            });
            System.Diagnostics.Debug.WriteLine(jwtoptions==null?true:false);
            services.AddSingleton(jwtoptions);


            services.AddScoped<SecurityInitializer>();
        }
        public static void AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
        {
            var rabbitMqHost = configuration["RabbitMq:Host"];
            var rabbitMqPort = configuration["RabbitMq:Port"];
            var rabbitMqUserName = configuration["RabbitMq:UserName"];
            var rabbitMqPassword = configuration["RabbitMq:Password"];

            services.AddMassTransit(config =>
            {
                config.UsingRabbitMq((context, cfg) =>
                {
                    var uri = string.Format("amqps://{0}:{1}@{2}:{3}", rabbitMqUserName, rabbitMqPassword, rabbitMqHost, rabbitMqPort);
                    cfg.Host(uri);                 
                });
            }
            );
        }
        public static void AddDatabase(IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DbConnectionString");

            services.AddDbContext<SecurityDbContext>(ctx =>
                ctx.UseSqlServer(connectionString));


            services.AddHostedService<DbInitializer>();
        }

        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMediatR(configuration => configuration.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
            services.AddAplication();
            AddDatabase(services, configuration);
            AddSecurity(services, configuration);
            AddRabbitMq(services, configuration);

            return services;
        }
    }
}
