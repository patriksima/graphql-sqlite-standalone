using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;

namespace GraphQLTest
{
    public class HttpServer : IDisposable
    {
        private static Queue<HttpListenerContext> _queue;
        private readonly HttpListener _listener;
        private readonly Thread _listenerThread;
        private readonly ManualResetEvent _stop, _ready;
        private readonly Thread[] _workers;

        public HttpServer(int maxThreads)
        {
            _workers = new Thread[maxThreads];
            _queue = new Queue<HttpListenerContext>();
            _stop = new ManualResetEvent(false);
            _ready = new ManualResetEvent(false);
            _listener = new HttpListener();
            _listenerThread = new Thread(HandleRequests);
        }

        public void Dispose()
        {
            Stop();
        }

        public void Start(int port)
        {
            _listener.Prefixes.Add($@"http://+:{port}/graphql/");
            _listener.Start();
            _listenerThread.Start();

            for (var i = 0; i < _workers.Length; i++)
            {
                _workers[i] = new Thread(Worker);
                _workers[i].Start();
            }
        }

        public void Stop()
        {
            _stop.Set();
            Console.WriteLine("_stop.Set()");
            _listenerThread.Join();
            Console.WriteLine("_listenerThread.Join()");
            foreach (var worker in _workers)
            {
                worker.Join();
                Console.WriteLine("worker.Join()");
            }

            _listener.Stop();
            Console.WriteLine("Server has stopped");
        }

        private void HandleRequests()
        {
            while (_listener.IsListening)
            {
                var result = _listener.BeginGetContext(ContextReady, _listener);
                if (0 == WaitHandle.WaitAny(new[] {_stop, result.AsyncWaitHandle}, 1000))
                {
                    return;
                }
            }
        }

        private void ContextReady(IAsyncResult ar)
        {
            try
            {
                var listener = (HttpListener) ar.AsyncState;
                lock (_queue)
                {
                    _queue.Enqueue(listener.EndGetContext(ar));
                    _ready.Set();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                throw;
            }
        }

        private void Worker()
        {
            if (_ready != null)
            {
                WaitHandle[] wait = {_ready, _stop};
                while (0 == WaitHandle.WaitAny(wait))
                {
                    HttpListenerContext context;
                    lock (_queue)
                    {
                        if (_queue.Count > 0)
                        {
                            context = _queue.Dequeue();
                        }
                        else
                        {
                            _ready.Reset();
                            continue;
                        }
                    }

                    try
                    {
                        ProcessRequest?.Invoke(context);
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine(e);
                    }
                }
            }
        }

        public event Action<HttpListenerContext> ProcessRequest;
    }
}