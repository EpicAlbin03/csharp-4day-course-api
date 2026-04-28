using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Text;
using BankingApi.Data;
using BankingApi.Models;
using BankingApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// builder.Services.AddDbContext<AppDbContext>(options =>
//     options.UseInMemoryDatabase("BankingDb"));

var connectionString = builder.Configuration.GetConnectionString("AppDb")
                       ?? throw new InvalidOperationException("Connection string 'AppDb' is not configured.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddSingleton<JwtTokenGenerator>();

// 1) Identity: user store + UserManager + PasswordHasher.
builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        // Dev-friendly password rules — relax for teaching, tighten for prod.
        options.Password.RequireDigit = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.Password.RequiredLength = 6;

        // Enforce unique emails — Identity doesn't do this by default.
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>();

// 2) JwtBearer: validate incoming tokens on every request.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SigningKey"]!)),
            NameClaimType = ClaimTypes.NameIdentifier
        };
    });

// 3) Authorization services ([Authorize] attribute machinery).
builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // db.Database.EnsureCreated();
    db.Database.Migrate();
    // DbSeeder.Seed(db);
    await DbSeeder.SeedAsync(
        db,
        scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>());
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
