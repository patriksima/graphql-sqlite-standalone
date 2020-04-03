using System;
using System.Net;

namespace GraphQLTest
{
    public class HttpServer : IDisposable
    {
        private readonly HttpListener _listener = new HttpListener();

        public HttpServer(int maxThreads)
        {
        }

        public void Dispose()
        {
            Stop();
        }

        public event Action<HttpListenerContext> OnProcess;

        public void Start(int port)
        {
            _listener.Prefixes.Add($@"http://+:{port}/graphql/");
            _listener.Start();
        }

        public void Listen()
        {
            while (_listener.IsListening)
            {
                var result = _listener.BeginGetContext(ar =>
                {
                    var listener = ar.AsyncState as HttpListener;
                    var context = listener?.EndGetContext(ar);
                    OnProcess?.Invoke(context);
                }, _listener);
                result.AsyncWaitHandle.WaitOne();
            }
        }

        public void Stop()
        {
            _listener.Stop();
            _listener.Close();
        }
    }
}