using BookNeighbors.Api.Data;
using BookNeighbors.Api.Interfaces;
using BookNeighbors.Api.Models.Identity;
using BookNeighbors.Api.Services;
 
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi


builder.Services.AddOpenApi();

builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme).AddIdentityCookies();
builder.Services.AddAuthorizationBuilder();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsql => npgsql.UseNetTopologySuite()
    )
);

builder.Services.AddIdentityCore<ApplicationUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddApiEndpoints();

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
builder.Services.AddControllers();

var app = builder.Build();
app.MapIdentityApi<ApplicationUser>();
app.MapControllers();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

      app.UseSwaggerUi(options =>
    {
        options.DocumentPath = "/openapi/v1.json";
    });
}
app.UseAuthentication();
app.UseAuthorization();
 
app.UseHttpsRedirection();
app.UseStaticFiles();
 

app.Run();

 