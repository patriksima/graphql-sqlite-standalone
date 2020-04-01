using System;
using System.Net;

namespace GraphQLTest
{
    public class MyServer : IDisposable
    {
        private readonly HttpListener _listener = new HttpListener();

        public Action<HttpListenerContext> OnAccept;

        public MyServer(int maxThreads)
        {
        }

        public void Dispose()
        {
            Stop();
        }

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
                    OnAccept?.Invoke(context);
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