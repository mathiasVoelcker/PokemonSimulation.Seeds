using System;
using System.Net.Http;
using System.Threading.Tasks;
using Extensions.MV;
using Newtonsoft.Json.Linq;
using PokemonSimulation.Infra.Interfaces;
using PokemonSimulation.Models;
using PokemonSimulation.Models.Enums;

namespace PokemonSimulation.Seeds
{
    public class NatureSeeds
    {
        private readonly INatureRepository _repository;

        private readonly string _apiUrl;

        public NatureSeeds(INatureRepository natureRepository, string apiUrl)
        {
            _repository = natureRepository;
            _apiUrl = apiUrl;
        }

        public async Task SeedsAsync() 
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiUrl);
                for (int i = 1; i <= 25; i++)
                {
                    var responseTask = client.GetAsync(string.Concat("nature/", i));    
                        responseTask.Wait();
                    var results = responseTask.Result;
                    if (results.IsSuccessStatusCode)
                    {
                        string apiResponse = await results.Content.ReadAsStringAsync();
                        var json = JObject.Parse(apiResponse);
                        var name = json["name"].ToString();
                        StatsEnum? increasedStats = null;
                        StatsEnum? decreasedStats = null;
                        try {
                            var increasedStatsString = json["increased_stat"]["name"].ToString();
                            if (!increasedStatsString.IsNullOrEmpty()) 
                                increasedStats = (StatsEnum)typeof(StatsEnum).GetEnumByDescription(increasedStatsString);
                            var decreasedStatsString = json["decreased_stat"]["name"].ToString();
                                decreasedStats = (StatsEnum)typeof(StatsEnum).GetEnumByDescription(decreasedStatsString);
                        } catch (Exception) {}
                        Natures nature = new Natures() 
                        {
                            Name = name,
                            Strong_Stat = increasedStats,
                            Weak_Stat = decreasedStats
                        };
                        _repository.CreateNature(nature);
                    }
                }
                
            }
        }
    }
}