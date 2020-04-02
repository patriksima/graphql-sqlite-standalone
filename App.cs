using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using GraphQL.SystemTextJson;
using GraphQL.Types;

namespace GraphQLTest
{
    public class App : IDisposable
    {
        private MyServer _server;
        private string _schema;

        public App()
        {
            LoadSchema();
        }

        public void Dispose()
        {
            _server.Stop();
            _server.OnAccept -= OnAcceptUtf8;
        }

        public void Run()
        {
            _server = new MyServer(3);
            _server.Start(9000);
            _server.OnAccept += OnAcceptUtf8;
            _server.Listen();
        }

        private async void LoadSchema()
        {
            using var reader = File.OpenText($@"D:\RiderProjects\GraphQLTest\schema.graphql");
            _schema = await reader.ReadToEndAsync();
        }

        private void OnAcceptUtf8(HttpListenerContext context)
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
            var js = JsonSerializer.Deserialize<GQuery>(ref utf8JsonReader,
                new JsonSerializerOptions {PropertyNameCaseInsensitive = true});

            Debug.WriteLine($"Query: {js.Query}");
            OnQuery(context, js.Query);
        }

        private async void OnQuery(HttpListenerContext context, string query)
        {
            var schema = Schema.For(_schema, _ =>
            {
                _.Types.Include<DroidType>();
                _.Types.Include<Query>();
            });


            var json = await schema.ExecuteAsync(_ => { _.Query = query; });

            Utility.Response(context.Response, json);
        }
    }
}