using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CsvHelper;
using GraphQL;
using GraphQL.Types;
using SQLite;


namespace GraphQLTest
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "graphql.db3");
            Console.WriteLine(path);
            var db = new SQLiteAsyncConnection(path);

            await db.DropTableAsync<DbUser>();
            await db.DropTableAsync<DbItem>();
            await db.DropTableAsync<DbUserItem>();

            await db.CreateTableAsync<DbUser>();
            await db.CreateTableAsync<DbItem>();
            await db.CreateTableAsync<DbUserItem>();

            // read demo data
            Console.WriteLine("Loading demo data...");
            using (var reader = new StreamReader(@"data\users.csv"))
            {
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                var items = csv.GetRecords<DbUser>();
                await db.InsertAllAsync(items);
            }

            using (var reader = new StreamReader(@"data\items.csv"))
            {
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                var items = csv.GetRecords<DbItem>();
                await db.InsertAllAsync(items);
            }

            using (var reader = new StreamReader(@"data\useritem.csv"))
            {
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                var items = csv.GetRecords<DbUserItem>();
                await db.InsertAllAsync(items);
            }

            Console.WriteLine("Searching users with name starts with P...");
            var query = db.Table<DbUser>().Where(v => v.Name.StartsWith("P"));

            await query.ToListAsync().ContinueWith((t) =>
            {
                foreach (var user in t.Result)
                    Console.WriteLine($"User: {user.Id}, {user.Name}, {user.LastName}, {user.Email}");
            });

            Console.WriteLine("Searching items with name starts with C...");
            var query2 = db.Table<DbItem>().Where(v => v.Name.StartsWith("C"));

            await query2.ToListAsync().ContinueWith((t) =>
            {
                foreach (var item in t.Result)
                    Console.WriteLine($"Item: {item.Id}, {item.Name}, {item.Class}, {item.Health}");
            });

            var app = new App();
            app.Run();
        }
    }
}