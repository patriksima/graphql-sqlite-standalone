using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.SystemTextJson;
using GraphQL.Types;

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
        public string Id(Droid droid) => droid.Id;
        public string Name(Droid droid) => droid.Name;

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
            App.Instance.Run();
        }
    }
}