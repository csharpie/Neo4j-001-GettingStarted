using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Neo4j_001_GettingStarted
{
    class Program
    {
        static void Main(string[] args)
        {
            var neo4jConfig = GetNeo4jConfig();

            var repository = new PersonRepository(neo4jConfig.URI, neo4jConfig.UserName, neo4jConfig.Password, true, true);

            var johnathan = repository.FindPerson("Johnathan");
            var suggestedFriendsForJohnathan = repository.SuggestFriends(johnathan);

            foreach (string suggestedFriend in suggestedFriendsForJohnathan)
            {
                Console.WriteLine($"{johnathan.Name} should become friends with {suggestedFriend}");
            }

            Console.ReadKey();
        }

        private static Neo4jConfig GetNeo4jConfig()
        {
            var builder = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json");

            var config = builder.Build();

            var neo4JConfig = config.GetSection("Neo4j").Get<Neo4jConfig>();

            return neo4JConfig;
        }
    }
}
