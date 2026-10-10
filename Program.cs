using System.Net;
using System.Text;
using System.Threading.RateLimiting;
using LoginService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token."
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [
                new OpenApiSecuritySchemeReference("Bearer", document)
            ] = []
        });
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),

            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],

            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddRateLimiter(options =>{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, cancellationToken) => {
        context.HttpContext.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails{
            Status = StatusCodes.Status429TooManyRequests,
            Title = "Too Many Requests",
            Detail = "Too many request. Please try again later."
        };

        await context.HttpContext.Response.WriteAsJsonAsync(
            problem,
            cancellationToken);
    };

    options.AddPolicy("LoginPolicy", httpContext =>
        CreateIpPolicy(5, TimeSpan.FromMinutes(1), httpContext));
    options.AddPolicy("RegisterPolicy", httpContext =>
        CreateIpPolicy(3, TimeSpan.FromMinutes(1), httpContext));
    options.AddPolicy("RefreshPolicy", httpContext =>
        CreateIpPolicy(10, TimeSpan.FromMinutes(1), httpContext));        
});  

builder.Services.AddDbContext<AuthDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJWTService,JWTService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();


static RateLimitPartition<string> CreateIpPolicy(
    int permitLimit,
    TimeSpan window,
    HttpContext httpContext)
{
    var ipAddress =
        httpContext.Connection.RemoteIpAddress?.ToString()
        ?? "unknown";

    return RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: ipAddress,
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = window,
            AutoReplenishment = true,
            QueueLimit = 0
        });
}


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "LoginService API v1");
    });
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
