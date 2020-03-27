using System;
using System.Collections.Generic;
using PokemonSimulation.Infra.Interfaces;
using PokemonSimulation.Infra.Repositories;

namespace PokemonSimulation.Seeds
{
    public class TypesSeeds
    {
        private ITypeRepository _typeRepository;

        private readonly List<Models.Type> _types = new List<Models.Type>() 
        {
            new Models.Type(){ Name = "Normal" },
            new Models.Type(){ Name = "Fire" },
            new Models.Type(){ Name = "Water" },
            new Models.Type(){ Name = "Electric" },
            new Models.Type(){ Name = "Grass" },
            new Models.Type(){ Name = "Ice" },
            new Models.Type(){ Name = "Fighting" },
            new Models.Type(){ Name = "Poison" },
            new Models.Type(){ Name = "Ground" },
            new Models.Type(){ Name = "Flying" },
            new Models.Type(){ Name = "Psychic" },
            new Models.Type(){ Name = "Bug" },
            new Models.Type(){ Name = "Rock" },
            new Models.Type(){ Name = "Ghost" },
            new Models.Type(){ Name = "Dragon" },
            new Models.Type(){ Name = "Dark" },
            new Models.Type(){ Name = "Steel" },
            new Models.Type(){ Name = "Fairy" }
        };

        public TypesSeeds(ITypeRepository typeRepository)
        {
            _typeRepository = typeRepository;
        }
        
        public void Seeds() 
        {
            foreach(var type in _types) 
            {
                _typeRepository.Create(type);
            }
        }
    }
}