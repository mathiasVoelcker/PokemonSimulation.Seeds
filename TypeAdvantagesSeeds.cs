using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PokemonSimulation.Infra.Interfaces;
using PokemonSimulation.Infra.Repositories;
using PokemonSimulation.Models;

namespace PokemonSimulation.Seeds
{
    public class TypeAdvantagesSeeds
    {

        private ITypeRepository _typeRepository;

        private ITypeAdvantageRepository _typeAdvantageRepository;

        private string _apiUrl;


        public TypeAdvantagesSeeds(ITypeRepository typeRepository, ITypeAdvantageRepository typeAdvantageRepository, string apiUrl)
        {
            _typeRepository = typeRepository;
            _typeAdvantageRepository = typeAdvantageRepository;
            _apiUrl = apiUrl;
        }

        public async Task SeedsAsync() 
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_apiUrl);
                
                var types = _typeRepository.GetAll();

                List<TypeAdvantage> typeAdvantages = new List<TypeAdvantage>();

                foreach (var type in types) 
                {
                    List<TypeAdvantage> typeAdvantagesByType = new List<TypeAdvantage>();
                    var errorCount = 0;
                    var serviceSuccess = false;
                    HttpResponseMessage results;
                    do {
                        var responseTask = client.GetAsync(string.Concat("type/", type.Name.ToLower()));    
                        responseTask.Wait();
                        results = responseTask.Result;
                        serviceSuccess = results.IsSuccessStatusCode;
                        errorCount++;
                        if (errorCount == 3)
                            throw new PokeApiException();
                    } while (!serviceSuccess);
                    string apiResponse = await results.Content.ReadAsStringAsync();
                    var json = JObject.Parse(apiResponse);
                    var damageRelations = json["damage_relations"]; 
                    var doubleDamageTo = damageRelations["double_damage_to"];
                    var halfDamageTo = damageRelations["half_damage_to"];
                    var noDamageTo = damageRelations["no_damage_to"];

                    foreach (var typeDoubleDamageTo in doubleDamageTo) 
                    {
                        var name = typeDoubleDamageTo["name"].ToString();
                        var idDefensiveType = types.First(x => x.Name.ToLower() == name.ToLower()).Id;
                        typeAdvantagesByType.Add(new TypeAdvantage(idAttackingType: type.Id, idDefensiveType: idDefensiveType, effect: 2));
                    }

                    foreach (var typeHaldDamageTo in halfDamageTo) 
                    {
                        var name = typeHaldDamageTo["name"].ToString();
                        var idDefensiveType = types.First(x => x.Name.ToLower() == name.ToLower()).Id;
                        typeAdvantagesByType.Add(new TypeAdvantage(idAttackingType: type.Id, idDefensiveType: idDefensiveType, effect: 0.5m));
                    }

                    foreach (var typeNoDamageTo in noDamageTo) 
                    {
                        var name = typeNoDamageTo["name"].ToString();
                        var idDefensiveType = types.First(x => x.Name.ToLower() == name.ToLower()).Id;
                        typeAdvantagesByType.Add(new TypeAdvantage(idAttackingType: type.Id, idDefensiveType: idDefensiveType, effect: 0m));
                    }

                    var neutralDamageTo = types.Where(x => !typeAdvantagesByType.Exists(y => x.Id == y.IdDefensiveType));

                    foreach (var defendingType in neutralDamageTo)
                        typeAdvantagesByType.Add(new TypeAdvantage(idAttackingType: type.Id, idDefensiveType: defendingType.Id, effect: 1m));

                    typeAdvantages.AddRange(typeAdvantagesByType);
                }

                foreach(var typeAdvantage in typeAdvantages) 
                    _typeAdvantageRepository.Create(typeAdvantage);

            }
        }
    }
}