#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Reflection;
using GraphQL.Types;
using SQLite;

namespace GraphQLTest
{
    public class Query
    {
        [GraphQLMetadata("user")]
        public DbUser GetUser(int id)
        {
            return new DbUser {Id = 666, Name = "Devil", LastName = "Himself", Email = "hell@gate.com"};
        }

        [GraphQLMetadata("users")]
        public IEnumerable<DbUser> GetUsers(int? itemid, string? fulltext)
        {
            Console.WriteLine($"itemId: {itemid}, fulltext: {fulltext}");
            return new List<DbUser>
            {
                new DbUser {Id = 666, Name = "Devil", LastName = "Himself", Email = "hell@gate.com"},
                new DbUser {Id = 999, Name = "Devil", LastName = "Alterego", Email = "hell@altergate.com"}
            };
        }

        [GraphQLMetadata("item")]
        public DbItem GetItem(int id)
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

    public sealed class MyUser : ObjectGraphType<DbUser>
    {
        public MyUser()
        {
            Field(user => user.Id).Name("id");
            Field(user => user.Name).Name("name");
            Field(user => user.LastName).Name("lastname");
            Field(user => user.Email).Name("email");
        }
    }

    public class MyQuery : ObjectGraphType
    {
        public MyQuery()
        {
            FieldAsync<MyUser>(
                "user",
                arguments: new QueryArguments(
                    new QueryArgument<IdGraphType> {Name = "id"}
                ),
                resolve: async context =>
                {
                    var id = context.GetArgument<int>("id");
                    var db = context.UserContext["db"] as SQLiteAsyncConnection;

                    return await db.Table<DbUser>().FirstOrDefaultAsync(x => x.Id == id);
                }
            );
        }
    }
}