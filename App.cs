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
        private static readonly Lazy<App>
            Lazy =
                new Lazy<App>
                    (() => new App());

        private MyServer _server;

        private App()
        {
        }

        public static App Instance => Lazy.Value;

        public void Dispose()
        {
            _server.OnAccept -= OnAcceptUtf8;
        }

        public void Run()
        {
            _server = new MyServer(3);
            _server.Start(9000);
            _server.OnAccept += OnAcceptUtf8;
            _server.Listen();
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
            var schema = Schema.For(@"
          type Droid {
            id: String!
            name: String!
            friend: Character
          }

          type Character {
            name: String!
          }

          type Query {
            hero: Droid
          }
        ", _ =>
            {
                _.Types.Include<DroidType>();
                _.Types.Include<Query>();
            });


            var json = await schema.ExecuteAsync(_ => { _.Query = query; });


            var response = context.Response;
            // Construct a response.
            var responseString = json;
            var buffer = Encoding.UTF8.GetBytes(responseString);
            // Get a response stream and write the response to it.
            response.ContentLength64 = buffer.Length;
            var output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            // You must close the output stream.
            output.Close();
        }
    }
}