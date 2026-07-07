using Application.Service.Emails;
using Domain.Identity;
using Express_Service;
using Express_Service.Hangfire;
using Express_Service.Hubs;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using QuestPDF.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

builder.Host.UseSerilog((context, services, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

builder.Services.AddControllers();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddHealthChecks();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("https://localhost:5001", "http://localhost:5000", "http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
builder.Services.AddDependencies(builder.Configuration);

var app = builder.Build();

if (builder.Configuration.GetValue("Seed:Enabled", false))
{
    await app.Services.SeedDataAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("UserId", httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "anonymous");
        diagnosticContext.Set("UserName", httpContext.User.Identity?.Name ?? "anonymous");
        diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
    };
});
app.UseAuthorization();
app.UseAntiforgery();
app.Use(async (context, next) =>
{
    var path = context.Request.Path;
    var isProtectedUiPath =
        path == "/" ||
        path.StartsWithSegments("/meetings") ||
        path.StartsWithSegments("/_blazor");

    if (isProtectedUiPath && context.User.Identity?.IsAuthenticated != true)
    {
        if (path.StartsWithSegments("/_blazor"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var returnUrl = context.Request.PathBase + context.Request.Path + context.Request.QueryString;
        context.Response.Redirect($"/login?returnUrl={Uri.EscapeDataString(returnUrl)}");
        return;
    }

    await next();
});
if (builder.Configuration.GetValue("Hangfire:Enabled", false))
{
    app.UseHangfireDashboard("/jobs", new DashboardOptions
    {
        Authorization = [new AdminHangfireDashboardAuthorizationFilter()],
        DashboardTitle = "Management System Jobs"
    });

    RecurringJob.AddOrUpdate<IEmailBackgroundJob>(
        "send-pending-emails",
        "emails",
        job => job.SendPendingEmailsAsync(),
        builder.Configuration["Hangfire:EmailRecurringCron"] ?? "*/5 * * * *",
        new RecurringJobOptions
        {
            TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time")
        });
}

app.MapControllers();
app.MapPost("/ui/login", async (HttpContext httpContext, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager) =>
{
    var form = await httpContext.Request.ReadFormAsync();
    var email = form["email"].ToString();
    var password = form["password"].ToString();
    var remember = form["remember"].ToString() == "on";
    var returnUrl = form["returnUrl"].ToString();
    var safeReturnUrl = string.IsNullOrWhiteSpace(returnUrl) || !returnUrl.StartsWith('/') || returnUrl.StartsWith("//")
        ? "/"
        : returnUrl;

    var user = await userManager.FindByEmailAsync(email);
    if (user is null || !user.IsActive)
        return Results.Redirect($"/login?error=1&returnUrl={Uri.EscapeDataString(safeReturnUrl)}");

    var result = await signInManager.PasswordSignInAsync(user, password, remember, lockoutOnFailure: false);
    return result.Succeeded
        ? Results.Redirect(safeReturnUrl)
        : Results.Redirect($"/login?error=1&returnUrl={Uri.EscapeDataString(safeReturnUrl)}");
})
.AllowAnonymous()
.DisableAntiforgery();

app.MapGet("/logout", async (SignInManager<ApplicationUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Redirect("/login");
}).RequireAuthorization();

app.MapRazorComponents<Express_Service.Components.App>()
    .AddInteractiveServerRenderMode();
app.MapHub<MeetingHub>("/hubs/meetings");
app.MapHealthChecks("/health");
app.MapGet("/api-docs", () => Results.Redirect("/swagger"));
app.Run();
