using System.Diagnostics;
using GraphQL.Types;
using SQLite;

namespace GraphQLTest
{
    public sealed class ItemType : ObjectGraphType<DbItem>
    {
        public ItemType()
        {
            Field(item => item.Id).Name("id");
            Field(item => item.Name).Name("name");
            Field(item => item.Class).Name("class");
            Field(item => item.Health).Name("health");
            FieldAsync<ListGraphType<UserType>>(
                "users",
                resolve: async context =>
                {
                    var db = context.UserContext["db"] as SQLiteAsyncConnection;

                    Debug.Assert(db != null, nameof(db) + " != null");
                    return await db.QueryAsync<DbUser>(
                        "select u.* from DbUser u inner join DbUserItem ui on ui.DbUserId = u.Id and ui.DbItemId = ?",
                        context.Source.Id);
                });
        }
    }
}