using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;

namespace PokemonSimulation.Seeds
{
    public static class LoggerDomain
    {

        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .Build();

        public static Logger GetLog(string connectionString)
        {
            return new LoggerConfiguration()
                    .ReadFrom.Configuration(Configuration)
                    .WriteTo
                    .MSSqlServer(connectionString, 
                        "LOGS", 
                        columnOptions: new ColumnOptions(),
                        autoCreateSqlTable: true,
                        restrictedToMinimumLevel: LogEventLevel.Error)
                    .CreateLogger();
        }
    }
}