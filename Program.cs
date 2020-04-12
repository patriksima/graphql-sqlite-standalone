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

            await CreateTables(db);
            await ImportDemoData(db);
            await SetupTriggers(db);

            var app = new App(db);
            app.Run();
        }

        private static async Task CreateTables(SQLiteAsyncConnection sqLiteAsyncConnection)
        {
            await sqLiteAsyncConnection.DropTableAsync<DbUser>();
            await sqLiteAsyncConnection.DropTableAsync<DbItem>();
            await sqLiteAsyncConnection.DropTableAsync<DbUserItem>();

            await sqLiteAsyncConnection.CreateTableAsync<DbUser>();
            await sqLiteAsyncConnection.CreateTableAsync<DbItem>();
            await sqLiteAsyncConnection.CreateTableAsync<DbUserItem>();

            await sqLiteAsyncConnection.ExecuteAsync("drop table userFulltext");
            await sqLiteAsyncConnection.ExecuteAsync("drop table itemFulltext");

            await sqLiteAsyncConnection.QueryAsync<DbUser>(
                "create virtual table userFulltext using fts5(Id, Name, LastName, Email)");
            await sqLiteAsyncConnection.QueryAsync<DbItem>(
                "create virtual table itemFulltext using fts5(Id, Name, Class, Health)");
        }

        private static async Task ImportDemoData(SQLiteAsyncConnection sqLiteAsyncConnection)
        {
            Console.Write("Loading demo data...");

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

            // copy data into fulltext tables
            await sqLiteAsyncConnection.ExecuteAsync(
                "insert into userFulltext(Id, Name, LastName, Email) select Id, Name, LastName, Email from DbUser");
            await sqLiteAsyncConnection.ExecuteAsync(
                "insert into itemFulltext(Id, Name, Class, Health) select Id, Name, Class, Health from DbItem");

            Console.WriteLine("done!");
        }


        private static async Task SetupTriggers(SQLiteAsyncConnection sqLiteAsyncConnection)
        {
            await sqLiteAsyncConnection.ExecuteAsync(
                @"CREATE TRIGGER IF NOT EXISTS insert_user 
                            AFTER INSERT ON DbUser 
                            BEGIN
                                INSERT INTO userFulltext (""Id"", ""Name"", ""LastName"", ""Email"")
                                VALUES (NEW.Id, NEW.Name, NEW.LastName, NEW.Email);
                            END;");

            await sqLiteAsyncConnection.ExecuteAsync(
                @"CREATE TRIGGER IF NOT EXISTS update_user
                            AFTER UPDATE ON DbUser
                            BEGIN
                                UPDATE userFulltext SET
                                Name = NEW.Name, 
                                LastName = NEW.LastName,
                                Email = NEW.Email
                                Where Id = NEW.Id;
                            END;");

            await sqLiteAsyncConnection.ExecuteAsync(
                @"CREATE TRIGGER IF NOT EXISTS delete_user
                            AFTER DELETE ON DbUser
                            BEGIN
                                DELETE FROM userFulltext WHERE Id = OLD.Id;
                            END;");

            await sqLiteAsyncConnection.ExecuteAsync(
                @"CREATE TRIGGER IF NOT EXISTS insert_item 
                            AFTER INSERT ON DbItem 
                            BEGIN
                                INSERT INTO itemFulltext (""Id"", ""Name"", ""Class"", ""Health"")
                                VALUES (NEW.Id, NEW.Name, NEW.Class, NEW.Health);
                            END;");

            await sqLiteAsyncConnection.ExecuteAsync(
                @"CREATE TRIGGER IF NOT EXISTS update_item
                            AFTER UPDATE ON DbItem
                            BEGIN
                                UPDATE itemFulltext SET
                                Name = NEW.Name, 
                                Class = NEW.Class,
                                Health = NEW.Health
                                Where Id = NEW.Id;
                            END;");

            await sqLiteAsyncConnection.ExecuteAsync(
                @"CREATE TRIGGER IF NOT EXISTS delete_item
                            AFTER DELETE ON DbItem
                            BEGIN
                                DELETE FROM itemFulltext WHERE Id = OLD.Id;
                            END;");
        }
    }
}