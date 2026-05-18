using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using SolveMachine.Models;
using SolveMachine.Repositories;
using SolveMachine.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpLogging(o => { });
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JwtParams:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JwtParams:Audience"],
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtParams:SecretKey"])),
            ValidateIssuerSigningKey = true
        };
    });

string dbConnectionString = builder.Configuration["DatabaseConnection:ConnectionString"];
builder.Services.AddDbContext<SolveMachineContext>(options => options
    .UseNpgsql(dbConnectionString, o =>
    {
        o.MapEnum<UserRole>("user_role");
        o.MapEnum<ProblemPriority>("problem_priority");
        o.MapEnum<ProblemStatus>("problem_status");
    }));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<ISelectionProblemRepository, SelectionProblemRepository>();
builder.Services.AddScoped<IModificationProblemRepository, ModificationProblemRepository>();
builder.Services.AddHostedService<ProblemBackgroundService>();

var app = builder.Build();

app.UseHttpLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/health", () => new { Message = "Welcome to 2021!!!!!!!!!!!!!!!!!!!!" });
app.MapControllers();

app.Run();