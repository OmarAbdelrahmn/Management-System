using System.Text;
using Application.Service.Boards;
using Application.Service.Auth;
using Application.Service.Emails;
using Application.Service.Invitations;
using Application.Service.Members;
using Application.Service.Meetings;
using Application.Service.Minutes;
using Application.Service.Permissions;
using Application.Service.Realtime;
using Application.Service.SystemCatalog;
using Application.Service.Voting;
using Domain;
using Domain.Auditing;
using Domain.Identity;
using Express_Service.Realtime;
using Express_Service.Services;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Express_Service;

public static class ApplicationDependencies
{
    public static IServiceCollection AddDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserContext, HttpCurrentUserContext>();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<ApplicationDbcontext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
            }));

        services.AddHangfire(config =>
        {
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.FromSeconds(15),
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true,
                    CommandTimeout = TimeSpan.FromMinutes(2)
                });
        });

        if (configuration.GetValue("Hangfire:Enabled", false))
        {
            services.AddHangfireServer(options =>
            {
                options.ServerName = "management-system";
                options.Queues = ["default", "emails"];
                options.WorkerCount = Math.Max(1, Environment.ProcessorCount);
            });
        }

        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
            .AddEntityFrameworkStores<ApplicationDbcontext>()
            .AddDefaultTokenProviders();

        var jwtKey = configuration["Jwt:Key"] ?? "development-key-change-before-production-development-key";
        services.Configure<AuthOptions>(configuration.GetSection("Jwt"));
        services.Configure<SmtpOptions>(configuration.GetSection("Smtp"));
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "SmartAuth";
                options.DefaultChallengeScheme = "SmartAuth";
            })
            .AddPolicyScheme("SmartAuth", "JWT or Identity cookie", options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    var authorization = context.Request.Headers.Authorization.ToString();
                    return authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                        ? JwtBearerDefaults.AuthenticationScheme
                        : IdentityConstants.ApplicationScheme;
                };
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"] ?? "ManagementSystem",
                    ValidAudience = configuration["Jwt:Audience"] ?? "ManagementSystemClients",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };
            });

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/login";
            options.LogoutPath = "/logout";
            options.AccessDeniedPath = "/access-denied";
            options.SlidingExpiration = true;
        });

        services.AddAuthorization();
        services.AddCascadingAuthenticationState();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IEmailBackgroundJob, EmailBackgroundJob>();
        services.AddScoped<IBoardAccessService, BoardAccessService>();
        services.AddScoped<IMeetingRealtimeNotifier, SignalRMeetingRealtimeNotifier>();
        services.AddScoped<IBoardService, BoardService>();
        services.AddScoped<IMeetingService, MeetingService>();
        services.AddScoped<IMemberService, MemberService>();
        services.AddScoped<IInvitationService, InvitationService>();
        services.AddScoped<IVotingService, VotingService>();
        services.AddScoped<IMinuteService, MinuteService>();
        services.AddScoped<ISystemCatalogService, SystemCatalogService>();
        services.Configure<SeedOptions>(configuration.GetSection("Seed"));
        services.AddScoped<DataSeeder>();
        services.AddScoped<MeetingUiService>();
        services.AddScoped<MemberUiService>();

        return services;
    }
}
