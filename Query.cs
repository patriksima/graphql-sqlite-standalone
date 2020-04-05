using System.Diagnostics;
using GraphQL;
using GraphQL.Types;
using SQLite;

namespace GraphQLTest
{
    public class Query : ObjectGraphType
    {
        public Query()
        {
            FieldAsync<UserType>(
                "user",
                arguments: new QueryArguments(
                    new QueryArgument<IdGraphType> {Name = "id"}
                ),
                resolve: async context =>
                {
                    var id = context.GetArgument<int>("id");
                    var db = context.UserContext["db"] as SQLiteAsyncConnection;

                    Debug.Assert(db != null, nameof(db) + " != null");
                    return await db.Table<DbUser>().FirstOrDefaultAsync(x => x.Id == id);
                }
            );

            FieldAsync<ItemType>(
                "item",
                arguments: new QueryArguments(
                    new QueryArgument<IdGraphType> {Name = "id"}
                ),
                resolve: async context =>
                {
                    var id = context.GetArgument<int>("id");
                    var db = context.UserContext["db"] as SQLiteAsyncConnection;

                    Debug.Assert(db != null, nameof(db) + " != null");
                    return await db.Table<DbItem>().FirstOrDefaultAsync(x => x.Id == id);
                }
            );
        }
    }
}