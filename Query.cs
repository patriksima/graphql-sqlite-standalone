using System.Collections.Generic;
using GraphQL;

namespace GraphQLTest
{
    public class Query
    {
        [GraphQLMetadata("user")]
        public DbUser GetUser()
        {
            return new DbUser {Id = 666, Name = "Devil", LastName = "Himself", Email = "hell@gate.com"};
        }

        [GraphQLMetadata("users")]
        public IEnumerable<DbUser> GetUsers()
        {
            return new List<DbUser>
            {
                new DbUser {Id = 666, Name = "Devil", LastName = "Himself", Email = "hell@gate.com"},
                new DbUser {Id = 999, Name = "Devil", LastName = "Alterego", Email = "hell@altergate.com"}
            };
        }

        [GraphQLMetadata("item")]
        public DbItem GetItem()
        {
            return new DbItem {Id = 666, Name = "Sword", Health = 100};
        }

        [GraphQLMetadata("items")]
        public IEnumerable<DbItem> GetItems()
        {
            return new List<DbItem>
            {
                new DbItem {Id = 666, Name = "Sword", Health = 100},
                new DbItem {Id = 999, Name = "Knife", Health = 100}
            };
        }
    }
}