using System.Collections.Generic;
using GraphQL;
using GraphQL.Types;

namespace GraphQLTest
{
    [GraphQLMetadata("Item", IsTypeOf = typeof(DbItem))]
    public class Item
    {
        public int Id(DbItem item) => item.Id;
        public string Name(DbItem item) => item.Name;
        public string Class(DbItem item) => item.Class;
        public int Health(DbItem item) => item.Health;

        public IEnumerable<DbUser> Users(ResolveFieldContext context, DbItem source)
        {
            return new List<DbUser>
            {
                new DbUser {Id = 666, Name = "Devil", LastName = "Lord", Email = "hell@gate.com"}
            };
        }
    }
}