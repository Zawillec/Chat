using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Chat.Application.Interfaces;
using Chat.Application.Settings;
using Chat.Infrastructure.Persistence;
using Chat.Infrastructure.Repositories;
using Chat.Infrastructure.Services;
using SoapCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//JWT Settings
var jwtSettingsSection = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettingsSection);
builder.Services.AddSingleton(jwtSettingsSection.Get<JwtSettings>());

//JWT Klucz i Walidacja
var key = Encoding.UTF8.GetBytes(jwtSettingsSection.Get<JwtSettings>().Key);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettingsSection["Issuer"],
        ValidAudience = jwtSettingsSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

//BAZA DANYCH
builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseInMemoryDatabase("ChatDb"));

//DI
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IMessageSoapService, MessageSoapService>();
builder.Services.AddScoped<MessageService>();

//Kontrolery REST
builder.Services.AddControllers();

//SOAP
builder.Services.AddSoapCore();

//Dostêp do HttpContext (dla SOAP autoryzacji)
builder.Services.AddHttpContextAccessor();

//Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Aplikacja
var app = builder.Build();

//Swagger Middleware
app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();
app.UseMiddleware<HeaderLoggingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    // REST API
    endpoints.MapControllers();

    // SOAP Endpoint
    endpoints.UseSoapEndpoint<IMessageSoapService>("/Service.svc",
        new SoapEncoderOptions(),
        SoapSerializer.DataContractSerializer);
});

app.Run();
