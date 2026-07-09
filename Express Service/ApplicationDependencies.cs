using Application.Service.Admin;
using Application.Service.Accounting;
using Application.Service.Auth;
using Application.Service.Beneficiaries;
using Application.Service.BeneficiaryServices;
using Application.Service.Boards;
using Application.Service.DocumentationArchive;
using Application.Service.ElectronicOffice;
using Application.Service.Emails;
using Application.Service.ExecutiveSupervision;
using Application.Service.EvaluationFollowUp;
using Application.Service.FinancialDevelopment;
using Application.Service.Invitations;
using Application.Service.HumanResources;
using Application.Service.InstitutionalExcellence;
using Application.Service.Meetings;
using Application.Service.Members;
using Application.Service.Messaging;
using Application.Service.Minutes;
using Application.Service.MovementMaintenance;
using Application.Service.Permissions;
using Application.Service.ProgramsProjects;
using Application.Service.PublicRelationsMedia;
using Application.Service.Realtime;
using Application.Service.ReportsStatistics;
using Application.Service.SystemCatalog;
using Application.Service.TaskManagement;
using Application.Service.TechEnablement;
using Application.Service.Volunteering;
using Application.Service.Voting;
using Domain;
using Domain.Auditing;
using Domain.Identity;
using Express_Service.Realtime;
using Express_Service.Services;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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
                    PrepareSchemaIfNecessary = true,
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
        services.AddScoped<IAccountingService, AccountingService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IEmailBackgroundJob, EmailBackgroundJob>();
        services.AddScoped<IBoardAccessService, BoardAccessService>();
        services.AddScoped<IMeetingRealtimeNotifier, SignalRMeetingRealtimeNotifier>();
        services.AddScoped<IBoardService, BoardService>();
        services.AddScoped<IMeetingService, MeetingService>();
        services.AddScoped<IMemberService, MemberService>();
        services.AddScoped<IBeneficiaryService, BeneficiaryService>();
        services.AddScoped<IBeneficiaryServicesService, BeneficiaryServicesService>();
        services.AddScoped<IDocumentationArchiveService, DocumentationArchiveService>();
        services.AddScoped<IElectronicOfficeService, ElectronicOfficeService>();
        services.AddScoped<IExecutiveSupervisionService, ExecutiveSupervisionService>();
        services.AddScoped<IEvaluationFollowUpService, EvaluationFollowUpService>();
        services.AddScoped<IFinancialDevelopmentService, FinancialDevelopmentService>();
        services.AddScoped<IHumanResourceService, HumanResourceService>();
        services.AddScoped<IInstitutionalExcellenceService, InstitutionalExcellenceService>();
        services.AddScoped<IMessagingService, MessagingService>();
        services.AddScoped<IMovementMaintenanceService, MovementMaintenanceService>();
        services.AddScoped<IProgramsProjectsService, ProgramsProjectsService>();
        services.AddScoped<IPublicRelationsMediaService, PublicRelationsMediaService>();
        services.AddScoped<IReportsStatisticsService, ReportsStatisticsService>();
        services.AddScoped<IInvitationService, InvitationService>();
        services.AddScoped<IVolunteeringService, VolunteeringService>();
        services.AddScoped<IVotingService, VotingService>();
        services.AddScoped<IMinuteService, MinuteService>();
        services.AddScoped<ISystemCatalogService, SystemCatalogService>();
        services.AddScoped<ITaskManagementService, TaskManagementService>();
        services.AddScoped<ITechEnablementService, TechEnablementService>();
        services.Configure<SeedOptions>(configuration.GetSection("Seed"));
        services.AddScoped<DataSeeder>();
        services.AddScoped<AccountingUiService>();
        services.AddScoped<MeetingUiService>();
        services.AddScoped<AdminUiService>();
        services.AddScoped<MemberUiService>();
        services.AddScoped<BeneficiaryUiService>();
        services.AddScoped<BeneficiaryServicesUiService>();
        services.AddScoped<DocumentationArchiveUiService>();
        services.AddScoped<ElectronicOfficeUiService>();
        services.AddScoped<ExecutiveSupervisionUiService>();
        services.AddScoped<EvaluationFollowUpUiService>();
        services.AddScoped<FinancialDevelopmentUiService>();
        services.AddScoped<HumanResourceUiService>();
        services.AddScoped<InstitutionalExcellenceUiService>();
        services.AddScoped<ProgramsProjectsUiService>();
        services.AddScoped<PublicRelationsMediaUiService>();
        services.AddScoped<ReportsStatisticsUiService>();
        services.AddScoped<SystemCatalogUiService>();
        services.AddScoped<MessagingUiService>();
        services.AddScoped<MovementMaintenanceUiService>();
        services.AddScoped<TechEnablementUiService>();
        services.AddScoped<VolunteeringUiService>();
        services.AddScoped<TaskUiService>();

        return services;
    }
}
