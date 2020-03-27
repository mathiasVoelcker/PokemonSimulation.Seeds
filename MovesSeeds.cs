using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Extensions.MV;
using Newtonsoft.Json.Linq;
using PokemonSimulation.Infra.Interfaces;
using PokemonSimulation.Models;
using PokemonSimulation.Models.Enums;

namespace PokemonSimulation.Seeds
{
    public class MovesSeeds
    {
        private readonly IMoveRepository _moveRepository;

        private readonly ITypeRepository _typeRepository;

        private readonly string _apiUrl;

        private readonly string _connectionString;

        public MovesSeeds(IMoveRepository moveRepository, 
            ITypeRepository typeRepository, 
            string apiUrl,
            string connectionString)
        {
            _moveRepository = moveRepository;
            _typeRepository = typeRepository;
            _apiUrl = apiUrl;
            _connectionString = connectionString;
        }

        public async Task SeedsAsync(int initialIndex)
        {
            var typeLists = _typeRepository.GetAll();
            int moveIndex = initialIndex;
            try {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_apiUrl);
                    for (int i = initialIndex; i <= 728; i++)
                    {
                        moveIndex = i;
                        HttpResponseMessage results;
                        bool responseSuccess = false;
                        int errorCount = 0;
                        do {
                            var responseTask = client.GetAsync(string.Concat("move/", i));    
                            responseTask.Wait();
                            results = responseTask.Result;
                            responseSuccess = results.IsSuccessStatusCode;
                            if (!responseSuccess)
                                errorCount++;
                            if (errorCount == 3)
                                throw new PokeApiException();

                        } while (responseSuccess == false);
                        
                        string apiResponse = await results.Content.ReadAsStringAsync();
                        var json = JObject.Parse(apiResponse);
                        var name = json["names"][2]["name"].ToString();
                        var basePowerString = json["power"].ToString();
                        var moveTypeString = json["type"]["name"].ToString();
                        var moveCategoryString = json["damage_class"]["name"].ToString();
                        
                        var basePower = basePowerString.ToDecimal();
                        var moveType = typeLists.FirstOrDefault(x => x.Name.ToLower() == moveTypeString);
                        var moveCategory = (MoveCategoryEnum)typeof(MoveCategoryEnum).GetEnumByDescription(moveCategoryString);
                        
                        if (moveCategory == MoveCategoryEnum.Status) continue;
                        
                        var move = new Moves()
                        {
                            Name = name,
                            Base_Power = basePower,
                            Move_Category = moveCategory,
                            Id_Type = moveType.Id
                        };

                        _moveRepository.Create(move);
                    }   
                }
            }
            catch (Exception ex) {
                using (var logger = LoggerDomain.GetLog(_connectionString))
                {
                    logger.Error(string.Format("Error at pokemonSpecies {0} - Exception Message: ", moveIndex, ex.Message));
                    throw ex;
                }
            }
        }
    }
}