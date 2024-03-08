using System;
using System.Net.Http;
using ApiApplication.Database;
using ApiApplication.Database.Repositories;
using ApiApplication.Database.Repositories.Abstractions;
using ApiApplication.Infrastructure.Grpc.MoviesApi;
using ApiApplication.Infrastructure.Cache;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProtoDefinitions;
using StackExchange.Redis;

namespace ApiApplication;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IShowtimesRepository, ShowtimesRepository>();
        services.AddTransient<ITicketsRepository, TicketsRepository>();
        services.AddTransient<IAuditoriumsRepository, AuditoriumsRepository>();
        services.AddTransient<IApiClientGrpc, ApiClientGrpc>();
        services.AddTransient<ICacheClient, CacheClient>();

        services.AddDbContext<CinemaContext>(options =>
        {
            options.UseInMemoryDatabase("CinemaDb")
                .EnableSensitiveDataLogging()
                .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning));
        });
        services.AddControllers();

        services.AddHttpClient();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));

        services.AddGrpcClient<MoviesApi.MoviesApiClient>(options =>
        {
            options.Address = new Uri(Configuration[ConfigurationKeyNames.MoviesApi.Uri]);
            options.ChannelOptionsActions.Add(channelOptions => channelOptions.HttpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });
        });

        services.AddTransient(_ =>
        {
            var redisUrl = Configuration[ConfigurationKeyNames.Redis.Url];
            var redisPort = Configuration[ConfigurationKeyNames.Redis.Port];
            var redisConnection = ConnectionMultiplexer.Connect($"{redisUrl}:{redisPort}");
            return redisConnection.GetDatabase();
        });

        services.AddHttpLogging(o => o.LoggingFields = HttpLoggingFields.Duration);
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        app.UseHttpLogging();

        SampleData.Initialize(app);
    }      
}