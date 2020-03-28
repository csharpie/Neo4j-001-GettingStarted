using System;
using System.Collections.Generic;
using System.Linq;
using Neo4j.Driver;

namespace Neo4j_001_GettingStarted
{
    public class PersonRepository : IDisposable
    {
        private readonly IDriver _driver;

        public PersonRepository(string uri, string userName, string password, bool shouldDeleteNodeAndRelationships, bool shouldSeedData = false)
        {
            _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(userName, password));

            if (shouldDeleteNodeAndRelationships) DeleteAllNodesAndRelationships();
            if (shouldSeedData) SeedData();
        }

        public Person CreatePerson(string name)
        {
            Person person = null;

            var statementText = "CREATE (they:Person) SET they.name = {name} RETURN they.name AS Name, id(they) AS Id";
            var statementParameters = new Dictionary<string, object> { { "name", name } };

            using (ISession session = _driver.Session())
            {
                var statementResult = session.Run(statementText, statementParameters);
                var result = statementResult.Single();

                person = new Person(result["Id"].As<int>(), result["Name"].As<string>());
               
            }

            return person;

        }

        public Person FindPerson(string name)
        {
            Person person = null;

            var statementText = "MATCH (they:Person) WHERE they.name = {name} RETURN they.name AS Name, id(they) AS Id";
            var statementParameters = new Dictionary<string, object> { { "name", name } };

            using (ISession session = _driver.Session())
            {
                var statementResult = session.Run(statementText, statementParameters);
                var result = statementResult.Single();

                person = new Person(result["Id"].As<int>(), result["Name"].As<string>());
            }

            return person;
        }

        public void MakeMutualFriends(Person they, Person them)
        {
            var statementText = "MATCH (they:Person), (them:Person) WHERE they.name = {theysName} AND them.name = {themsName} CREATE (they)-[fw:FRIENDS]->(them) RETURN fw";
            var statementParameters = new Dictionary<string, object> { { "theysName", they.Name }, { "themsName", them.Name } };
           
            using (ISession session = _driver.Session())
            {
                session.Run(statementText, statementParameters);
            }
        }

        public List<string> SuggestFriends(Person they)
        {
            var suggestedFriends = new List<string>();

            var statementText = "MATCH (they:Person)-[f:FRIENDS]->(friend:Person)-[:FRIENDS*1..2]->(friendOfFriend:Person) WHERE they.name = {name} RETURN friendOfFriend.name AS Name";
            var statementParameters = new Dictionary<string, object> { { "name", they.Name } };

            using (ISession session = _driver.Session())
            {
                var statementResult = session.Run(statementText, statementParameters);

                suggestedFriends.AddRange(statementResult.ToList().Select(suggestedFriend => suggestedFriend["Name"].As<string>()));

                return suggestedFriends;
            }
        }

        public void Dispose()
        {
            _driver?.Dispose();
        }

        private void SeedData()
        {
            var johnathan = CreatePerson("Johnathan");
            var mark = CreatePerson("Mark");
            var phil = CreatePerson("Phil");
            var mary = CreatePerson("Mary");
            var luke = CreatePerson("Luke");
            MakeMutualFriends(johnathan, mark);
            MakeMutualFriends(mark, mary);
            MakeMutualFriends(mary, phil);
            MakeMutualFriends(phil, mary);
            MakeMutualFriends(phil, luke);
        }

        private void DeleteAllNodesAndRelationships()
        {
            var statementText = "MATCH (n) DETACH DELETE n";

            using (ISession session = _driver.Session())
            {
                session.Run(statementText);
            }
        }
    }
}