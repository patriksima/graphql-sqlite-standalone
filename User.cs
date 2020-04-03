using System.Collections.Generic;
using GraphQL;
using GraphQL.Types;

namespace GraphQLTest
{
    [GraphQLMetadata("User", IsTypeOf = typeof(DbUser))]
    public class User
    {
        public int Id(DbUser user) => user.Id;
        public string Name(DbUser user) => user.Name;
        public string LastName(DbUser user) => user.LastName;
        public string Email(DbUser user) => user.Email;

        public IEnumerable<DbItem> Items(ResolveFieldContext context, DbUser source)
        {
            return new List<DbItem>
            {
                new DbItem {Id = 666, Class = "Hellish", Health = 100, Name = "Hell Item"}
            };
        }
    }
}