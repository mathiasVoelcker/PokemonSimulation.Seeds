using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.FileExtensions;
using Microsoft.Extensions.Configuration.Json;
using PokemonSimulation.Infra.Database;
using PokemonSimulation.Infra.Repositories;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;

namespace PokemonSimulation.Seeds
{
    class Program
    {

        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .Build();

        static async System.Threading.Tasks.Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            var config = builder.Build();
            var connectionString = config.GetConnectionString("DefaultConnection");
            
            int pokemonSpeciesIndex;
            int moveIndex;
            int tableStartInt;
            try {
                tableStartInt = Int32.Parse(args[0]);
                pokemonSpeciesIndex = Int32.Parse(args[1]);
                moveIndex = Int32.Parse(args[2]);
            }
            catch (Exception) 
            {
                using(var logger = LoggerDomain.GetLog(connectionString))
                {
                    logger.Error("Arguments must be set and be a integer");
                    throw new Exception("Arguments must be set and be a integer");
                }
            }

            var dbSession = new DbSession(config.GetConnectionString("DefaultConnection"));

            var typesRepository = new TypeRepository(dbSession);
            var typesAdvantageRepository = new TypeAdvantageRepository(dbSession);
            var natureRepository = new NatureRepository(dbSession);
            var pokemonSpeciesRepository = new PokemonSpeciesRepository(dbSession);
            var moveRepository = new MoveRepository(dbSession);
            var apiUrl = config["apiURL"];

            var typeSeeds = new TypesSeeds(typesRepository);
            var typeAdvantagesSeeds = new TypeAdvantagesSeeds(typesRepository, typesAdvantageRepository, apiUrl);
            var natureSeeds = new NatureSeeds(natureRepository, apiUrl);
            var pokemonSpeciesSeeds = new PokemonSpeciesSeeds(pokemonSpeciesRepository, typesRepository, apiUrl, connectionString);
            var moveSeeds = new MovesSeeds(moveRepository, typesRepository, apiUrl, connectionString);

            if (tableStartInt <= (int)TableEnum.Type)
                typeSeeds.Seeds();
            if (tableStartInt <= (int)TableEnum.TypeAdvantage)
                await typeAdvantagesSeeds.SeedsAsync();
            if (tableStartInt <= (int)TableEnum.Nature)
                await natureSeeds.SeedsAsync();
            if (tableStartInt <= (int)TableEnum.PokemonSpecies)
                await pokemonSpeciesSeeds.SeedsAsync(pokemonSpeciesIndex);
            if (tableStartInt <= (int)TableEnum.Move)
                await moveSeeds.SeedsAsync(moveIndex);

        }


    }
}
