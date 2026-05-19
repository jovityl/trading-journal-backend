using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TradingJournal.Application.Interfaces;
using TradingJournal.Domain.Interfaces;
using TradingJournal.Domain.IRepository;
using TradingJournal.Infrastructure.Implementations;
using TradingJournal.Infrastructure.Persistence;
using TradingJournal.Infrastructure.Repository;
using TradingJournal.Infrastructure.Services;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<TradingJournalDbContext>(options =>
    options.UseNpgsql(config.GetConnectionString("DefaultConnection")));

// MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(TradingJournal.Application.Handlers.Trades.Queries.GetTradesQueryHandler).Assembly));

// Repositories
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITradeRepository, TradeRepository>();
builder.Services.AddScoped<IPromptRepository, PromptRepository>();
builder.Services.AddScoped<ITokenUsageRepository, TokenUsageRepository>();
builder.Services.AddScoped<ITradeMessageRepository, TradeMessageRepository>();

// Services
builder.Services.AddScoped<IStorageService, LocalStorageService>();
builder.Services.AddSingleton<IAdminSettings, AdminSettings>();
builder.Services.AddSingleton<IPromptService, PromptService>();
builder.Services.AddScoped<ITokenUsageService, TokenUsageService>();
builder.Services.AddHttpClient<IAiScoringService, ClaudeAiScoringService>();
// Register both implementations as concrete types so router can pick between them
builder.Services.AddHttpClient<ClaudeChatService>();
builder.Services.AddHttpClient<OpenRouterChatService>();
builder.Services.AddScoped<IChatServiceRouter, ChatServiceRouter>();
// IChatService default → use router's choice (default Claude)
builder.Services.AddScoped<IChatService>(sp => sp.GetRequiredService<IChatServiceRouter>().Resolve(null));
builder.Services.AddHttpClient<IChatModerationService, ChatModerationService>();

// Auth0
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://{config["Auth0:Domain"]}/";
        options.Audience = config["Auth0:Audience"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.Requirements.Add(new TradingJournal.Api.Authorization.AdminRequirement()));
});
builder.Services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, TradingJournal.Api.Authorization.AdminRequirementHandler>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowAnyOrigin());
});

var app = builder.Build();

// Seed prompts on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TradingJournalDbContext>();
    await TradingJournal.Infrastructure.Persistence.PromptSeeder.SeedAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// serve uploaded screenshots from /uploads/* path
app.UseStaticFiles(new Microsoft.AspNetCore.Builder.StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "uploads")),
    RequestPath = "/uploads"
});
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
