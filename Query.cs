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

            FieldAsync<ListGraphType<UserType>>(
                "users",
                arguments: new QueryArguments(
                    new QueryArgument<IdGraphType> {Name = "itemid"},
                    new QueryArgument<StringGraphType> {Name = "fulltext"}
                ),
                resolve: async context =>
                {
                    var itemId = context.GetArgument<int>("itemid");
                    var fulltext = context.GetArgument<string>("fulltext");
                    var db = context.UserContext["db"] as SQLiteAsyncConnection;

                    Debug.Assert(db != null, nameof(db) + " != null");

                    if (itemId > 0 && !string.IsNullOrEmpty(fulltext))
                    {
                        return await db.QueryAsync<DbUser>(@"
                            select u.* from DbUser u 
                            inner join DbUserItem ui on DbItemId = ? AND DbUserId = u.Id
                            inner join userFulltext f on f.Id = u.Id
                            where userFulltext MATCH ?", itemId, fulltext);
                    }

                    if (itemId > 0)
                    {
                        return await db.QueryAsync<DbUser>(@"
                            select u.* from DbUser u 
                            inner join DbUserItem ui on DbItemId = ? AND DbUserId = u.Id", itemId);
                    }

                    if (!string.IsNullOrEmpty(fulltext))
                    {
                        return await db.QueryAsync<DbUser>(@"
                            select u.* from DbUser u 
                            inner join userFulltext f on f.Id = u.Id 
                            where userFulltext MATCH ?", fulltext);
                    }

                    return await db.QueryAsync<DbUser>(@"select * from DbUser");
                }
            );

            FieldAsync<ListGraphType<ItemType>>(
                "items",
                arguments: new QueryArguments(
                    new QueryArgument<IdGraphType> {Name = "userid"},
                    new QueryArgument<StringGraphType> {Name = "fulltext"}
                ),
                resolve: async context =>
                {
                    var userId = context.GetArgument<int>("userid");
                    var fulltext = context.GetArgument<string>("fulltext");
                    var db = context.UserContext["db"] as SQLiteAsyncConnection;

                    Debug.Assert(db != null, nameof(db) + " != null");

                    if (userId > 0 && !string.IsNullOrEmpty(fulltext))
                    {
                        return await db.QueryAsync<DbItem>(@"
                            select u.* from DbItem u 
                            inner join DbUserItem ui on DbItemId = u.Id AND DbUserId = ?
                            inner join itemFulltext f on f.Id = u.Id
                            where itemFulltext MATCH ?", userId, fulltext);
                    }

                    if (userId > 0)
                    {
                        return await db.QueryAsync<DbItem>(@"
                            select u.* from DbItem u 
                            inner join DbUserItem ui on DbItemId = u.Id AND DbUserId = ?", userId);
                    }

                    if (!string.IsNullOrEmpty(fulltext))
                    {
                        return await db.QueryAsync<DbItem>(@"
                            select u.* from DbItem u 
                            inner join itemFulltext f on f.Id = u.Id 
                            where itemFulltext MATCH ?", fulltext);
                    }

                    return await db.QueryAsync<DbItem>(@"select * from DbItem");
                }
            );
        }
    }
}