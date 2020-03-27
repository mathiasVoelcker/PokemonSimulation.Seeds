using System;

namespace PokemonSimulation.Seeds
{
    public class PokeApiException : Exception
    {
        public PokeApiException() : base (message: "Error on pokeapi server. Try again later") { }
    }
}