using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Xml.Schema;
using GraphQL.SystemTextJson;
using GraphQL.Types;
using SQLite;

namespace GraphQLTest
{
    public class App : IDisposable
    {
        private SQLiteAsyncConnection _db;
        private HttpServer _server;
        private string _schema;

        public App(SQLiteAsyncConnection db)
        {
            _db = db;
            LoadSchema();
        }

        public void Dispose()
        {
            _server.Stop();
            _server.OnProcess -= OnProcessUtf8;
        }

        public void Run()
        {
            _server = new HttpServer(3);
            _server.Start(9000);
            _server.OnProcess += OnProcessUtf8;
            _server.Listen();
        }

        private async void LoadSchema()
        {
            using var reader = File.OpenText($@"D:\RiderProjects\GraphQLTest\schema.graphql");
            _schema = await reader.ReadToEndAsync();
        }

        private void OnProcessUtf8(HttpListenerContext context)
        {
            var request = context.Request;
            if (!request.HasEntityBody)
            {
                return;
            }

            using var body = request.InputStream;
            using var reader = new StreamReader(body, request.ContentEncoding);
            var content = reader.ReadToEnd();

            Debug.WriteLine($"Content: {content}");

            var utf8JsonReader = new Utf8JsonReader(Encoding.ASCII.GetBytes(content));
            var js = JsonSerializer.Deserialize<GraphQuery>(ref utf8JsonReader,
                new JsonSerializerOptions {PropertyNameCaseInsensitive = true});

            Debug.WriteLine($"Query: {js.Query}");
            OnQuery(context, js.Query);
        }

        private async void OnQuery(HttpListenerContext context, string query)
        {
            var schema = new Schema {Query = new Query()};
            var json = await schema.ExecuteAsync(_ =>
            {
                _.Query = query;
                _.UserContext = new Dictionary<string, object>
                {
                    {"db", _db}
                };
                _.ExposeExceptions = true;
            });

            Utility.Response(context.Response, json);
        }
    }
}