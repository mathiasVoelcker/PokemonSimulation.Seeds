using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PokemonSimulation.Infra.Interfaces;
using PokemonSimulation.Models;

namespace PokemonSimulation.Seeds
{
    public class PokemonSpeciesSeeds
    {
        private IPokemonSpeciesRepository _pokemonRepository;

        private ITypeRepository _typeRepository;

        private string _apiUrl;

        private string _connectionString;

        public PokemonSpeciesSeeds(IPokemonSpeciesRepository pokemonSpeciesRepository, 
            ITypeRepository typeRepository, 
            string apiUrl,
            string connectionString) 
        {
            _pokemonRepository = pokemonSpeciesRepository;
            _typeRepository = typeRepository;
            _apiUrl = apiUrl;
            _connectionString = connectionString;
        }

        public async Task SeedsAsync(int initialIndex)
        {
            int pokemonNationalNumb = initialIndex;
            try {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_apiUrl);
                    var allTypes = _typeRepository.GetAll();

                    for (int i = initialIndex; i <= 807; i++) 
                    {
                        pokemonNationalNumb = i;
                        HttpResponseMessage results;
                        bool responseSuccess = false;
                        int errorCount = 0;
                        do {
                            var responseTask = client.GetAsync(string.Concat("pokemon/", i));
                            responseTask.Wait();
                            results = responseTask.Result;
                            responseSuccess = results.IsSuccessStatusCode;
                            if (!responseSuccess) errorCount++;
                            if (errorCount == 3)
                                throw new PokeApiException();
                        } while(!responseSuccess);
                        string apiResponse = await results.Content.ReadAsStringAsync();
                        var json = JObject.Parse(apiResponse);
                        var name = json["forms"][0]["name"].ToString();
                        var stats = json["stats"];
                        var types = json["types"];
                        var firstType = allTypes.FirstOrDefault(x => x.Name.ToLower() == types[0]["type"]["name"].ToString().ToLower());
                        var secondType = new Models.Type();
                        
                        var pokemonSpecies = new Pokemon_Species() 
                        {
                            National_Numb = i,
                            Name = name,
                            Base_Hp = (int)stats[5]["base_stat"],
                            Base_Attack = (int)stats[4]["base_stat"],
                            Base_Defense = (int)stats[3]["base_stat"],
                            Base_Sp_Attack = (int)stats[2]["base_stat"],
                            Base_Sp_Defense = (int)stats[1]["base_stat"],
                            Base_Speed = (int)stats[0]["base_stat"],
                            First_Type_Id = firstType.Id,
                        };
                        
                        if (types.Count() > 1) 
                        {
                            secondType = allTypes.FirstOrDefault(x => x.Name.ToLower() == types[1]["type"]["name"].ToString().ToLower());
                            pokemonSpecies.Second_Type_Id = secondType.Id;
                        }
                        _pokemonRepository.Create(pokemonSpecies);
                    
                    }
                }
            } 
            catch (Exception ex) 
            {
                using (var logger = LoggerDomain.GetLog(_connectionString)) 
                {
                    logger.Error(string.Format("Error at pokemonSpecies {0} - Exception Message: ", pokemonNationalNumb, ex.Message));
                    throw ex;
                }
            }
        }

    }
}