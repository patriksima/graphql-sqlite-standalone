using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CsvHelper;
using SQLite;

namespace GraphQLTest
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "graphql.db3");

            var db = new SQLiteAsyncConnection(path);

            await ImportDemoData(db);

            var app = new App(db);
            app.Run();
        }

        private static async Task ImportDemoData(SQLiteAsyncConnection sqLiteAsyncConnection)
        {
            await sqLiteAsyncConnection.DropTableAsync<DbUser>();
            await sqLiteAsyncConnection.DropTableAsync<DbItem>();
            await sqLiteAsyncConnection.DropTableAsync<DbUserItem>();

            await sqLiteAsyncConnection.CreateTableAsync<DbUser>();
            await sqLiteAsyncConnection.CreateTableAsync<DbItem>();
            await sqLiteAsyncConnection.CreateTableAsync<DbUserItem>();

            // read demo data
            Console.WriteLine("Loading demo data...");
            using (var reader = new StreamReader(@"data\users.csv"))
            {
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                var items = csv.GetRecords<DbUser>();
                await sqLiteAsyncConnection.InsertAllAsync(items);
            }

            using (var reader = new StreamReader(@"data\items.csv"))
            {
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                var items = csv.GetRecords<DbItem>();
                await sqLiteAsyncConnection.InsertAllAsync(items);
            }

            using (var reader = new StreamReader(@"data\useritem.csv"))
            {
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                var items = csv.GetRecords<DbUserItem>();
                await sqLiteAsyncConnection.InsertAllAsync(items);
            }
        }
    }
}