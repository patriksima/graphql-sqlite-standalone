using System;
using GraphQL;
using GraphQL.Types;
using Microsoft.Data.Sqlite;

namespace GraphQLTest
{
    public class GQuery
    {
        public string Query { get; set; }
    }

    public class Droid
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class Character
    {
        public string Name { get; set; }
    }

    public class Query
    {
        [GraphQLMetadata("hero")]
        public Droid GetHero()
        {
            return new Droid {Id = "1", Name = "R2-D2"};
        }
    }

    [GraphQLMetadata("Droid", IsTypeOf = typeof(Droid))]
    public class DroidType
    {
        public string Id(Droid droid)
        {
            return droid.Id;
        }

        public string Name(Droid droid)
        {
            return droid.Name;
        }

        // these two parameters are optional
        // ResolveFieldContext provides contextual information about the field
        public Character Friend(ResolveFieldContext context, Droid source)
        {
            return new Character {Name = "C3-PO"};
        }
    }

    public static class Program
    {
        public static void Main(string[] args)
        {
            const string cs = "Data Source=:memory:";
            const string stm = "SELECT SQLITE_VERSION()";

            var connection = new SqliteConnection(cs);
            connection.Open();

            var cmd = new SqliteCommand(stm, connection);
            var version = cmd.ExecuteScalar().ToString();

            Console.WriteLine($"SQLite version: {version}");

            var app = new App();
            app.Run();
        }
    }
}