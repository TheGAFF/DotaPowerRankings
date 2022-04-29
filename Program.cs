using System.Net;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Extensions.Http;
using RD2LPowerRankings.Database;
using RD2LPowerRankings.Database.Dota;
using RD2LPowerRankings.Modules.Dota;
using RD2LPowerRankings.Modules.GoogleSheets;
using RD2LPowerRankings.Services.Common;
using RD2LPowerRankings.Services.DotaDataSource;
using RD2LPowerRankings.Services.DotaRanking;
using RD2LPowerRankings.Services.PlayerDataSource;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DotaDbContext>(x =>
    x.UseNpgsql(builder.Configuration.GetConnectionString("DotaPowerRankings")), ServiceLifetime.Transient);

builder.Services.AddTransient<IGoogleSheetsService, GoogleSheetsService>();
builder.Services.AddTransient<IDotaDataSource, OpenDotaDotaDataSource>();
builder.Services.AddTransient<IPlayerDataSource, RD2LPlayerDataSource>();
builder.Services.AddTransient<IDotaRankingService, DotaRankingService>();
builder.Services.AddTransient<IPostSeasonAwardService, PostSeasonAwardService>();
builder.Services
    .AddHttpClient<OpenDotaDotaDataSource>()
    .SetHandlerLifetime(TimeSpan.FromMinutes(60))
    .AddPolicyHandler((services, request) => HttpPolicyExtensions.HandleTransientHttpError()
        .OrResult(response => response.StatusCode == HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(HttpRetryPolicies.GetBasicJitterRetryPolicy(),
            (outcome, timespan, retryAttempt, context) =>
            {
                services.GetService<ILogger<OpenDotaDotaDataSource>>()?
                    .LogWarning("Delaying for {Delay}secs, then making retry #{Retry}", timespan.Seconds,
                        retryAttempt);
            }
        ));

//builder.Services.AddAutoMapper(typeof(DotaMappingProfile));
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<DotaDbContext>();
        DotaDbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database");
    }
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();