using BookNeighbors.Api.Data;
using BookNeighbors.Api.Interfaces;
using BookNeighbors.Api.Models.Identity;
using BookNeighbors.Api.Services;
using BookNeighbors.Api.Validators;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using BookNeighbors.Api.Hubs;
using BookNeighbors.Api.Models;




var builder = WebApplication.CreateBuilder(args);
// Configurazione della lettura della configurazione dell'applicazione
builder.Services.Configure<AppOptions>(builder.Configuration.GetSection("AppOptions"));
// Configurazione dei servizi per l'applicazione
builder.Services.AddOpenApi();

// Configurazione dei servizi per la gestione dei token CSRF
builder.Services.AddSignalR();
//   Configurazione del database PostgreSQL con NetTopologySuite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsql => npgsql.UseNetTopologySuite()
    )
);
// Configurazione dei servizi Identity

builder.Services.AddIdentityCore<ApplicationUser>().AddRoles<IdentityRole<Guid>>().AddEntityFrameworkStores<ApplicationDbContext>().AddApiEndpoints();


// Configurazione dei servizi custom

builder.Services.AddScoped<ILibraryService, LibraryService>();
builder.Services.AddScoped<IGeoLocationService, GeoLocationService>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddHttpClient<IIsbnLookupService, IsbnLookupService>(
client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddScoped<ICoverImageService, CoverImageService>();
builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<IUserValidator<ApplicationUser>, ApplicationUserValidator>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ILoanMessageService, LoanMessageService>();
builder.Services.AddScoped<IPersonalLoanStatisticsService, PersonalLoanStatisticsService>();
builder.Services.AddScoped<INotificationRealtimePublisher, NotificationRealtimePublisher>();
builder.Services.AddScoped<ICoverUrlResolver, CoverUrlResolver>();
// Configurazione dei servizi per la gestione dei token CSRF

builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme).AddIdentityCookies();
builder.Services.AddAuthorizationBuilder();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(
        new AutoValidateAntiforgeryTokenAttribute());
});

var allowedOrigins = builder.Configuration.GetSection("AppOptions:Client:AllowedOrigins").Get<string[]>() ?? throw new InvalidOperationException("AppOptions:Client:AllowedOrigins non configurato.");

if (allowedOrigins.Length == 0) throw new InvalidOperationException("Nessuna origine CORS configurata.");

builder.Services.AddCors(options =>
{
    options.AddPolicy("Blazor", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
    options.Cookie.Name = "BookNeighbors.Antiforgery";
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});


var app = builder.Build();

// Configurazione iniziale del ruolo amministratore
// utilizzata esclusivamente in sviluppo.
if (app.Environment.IsDevelopment())
{
    var adminEmail = app.Configuration["BootstrapAdminEmail"];

    if (!string.IsNullOrWhiteSpace(adminEmail))
    {
        using var scope = app.Services.CreateScope();

        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            var result = await roleManager.CreateAsync(
                new IdentityRole<Guid>("Admin"));

            if (!result.Succeeded)
                throw new InvalidOperationException(
                    "Impossibile creare il ruolo Admin.");
        }

        var admin = await userManager.FindByEmailAsync(adminEmail);

        if (admin is not null &&
            !await userManager.IsInRoleAsync(admin, "Admin"))
        {
            var result = await userManager.AddToRoleAsync(
                admin, "Admin");

            if (!result.Succeeded)
                throw new InvalidOperationException(
                    "Impossibile assegnare il ruolo Admin.");
        }
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("Blazor");

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUi(options =>
    {
        options.DocumentPath = "/openapi/v1.json";
    });
}

// Endpoint per recuperare il token CSRF
app.MapGet("/api/security/antiforgery-token",
    (HttpContext context, IAntiforgery antiforgery) =>
    {
        context.Response.Headers.CacheControl = "no-store";

        var tokens = antiforgery.GetAndStoreTokens(context);

        return Results.Ok(new
        {
            token = tokens.RequestToken
        });
    });

// Endpoint Identity
app.MapIdentityApi<ApplicationUser>();

// Controller API
app.MapControllers();
// Endpoint SignalR per la chat dei prestiti
app.MapHub<LoanChatHub>("/hubs/loan-chat").RequireAuthorization();


// Logout protetto da CSRF
app.MapPost("/logout", async (
    HttpContext context,
    IAntiforgery antiforgery,
    SignInManager<ApplicationUser> signInManager) =>
{
    if (!await antiforgery.IsRequestValidAsync(context))
        return Results.BadRequest("Token CSRF non valido.");

    await signInManager.SignOutAsync();

    return Results.Ok();
})
.RequireAuthorization();

app.Run();



