using System.Net;
using System.Text;

namespace GraphQLTest
{
    public class Utility
    {
        public static void Response(HttpListenerResponse listenerResponse, string responseString)
        {
            var buffer = Encoding.UTF8.GetBytes(responseString);

            listenerResponse.ContentLength64 = buffer.Length;
            var output = listenerResponse.OutputStream;

            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }
    }
}