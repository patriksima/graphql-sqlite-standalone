using System.Diagnostics;
using GraphQL.Types;
using SQLite;

namespace GraphQLTest
{
    public sealed class UserType : ObjectGraphType<DbUser>
    {
        public UserType()
        {
            Field(user => user.Id).Name("id");
            Field(user => user.Name).Name("name");
            Field(user => user.LastName).Name("lastname");
            Field(user => user.Email).Name("email");
            FieldAsync<ListGraphType<ItemType>>(
                "items",
                resolve: async context =>
                {
                    var db = context.UserContext["db"] as SQLiteAsyncConnection;

                    Debug.Assert(db != null, nameof(db) + " != null");
                    return await db.QueryAsync<DbItem>(
                        "select i.* from DbItem i inner join DbUserItem ui on ui.DbUserId = ? and ui.DbItemId = i.Id",
                        context.Source.Id);
                });
        }
    }
}