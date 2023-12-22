using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame
{
    internal class RequestHandler
    {
        public Socket EstablishConnection()//TODO make own thing maybe to avoid circular bullshit
        {
            Socket serverSocket = new Socket(AddressFamily.InterNetwork,
                                            SocketType.Stream,
                                            ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(IPAddress.Loopback, 10001));
            serverSocket.Listen(6);

            return serverSocket.Accept();
        }

        public void ProcessInput()
        {
            MethodHandler methodHandler = new MethodHandler();
            byte[] buffer = new byte[256];

            while(true)
            {
                Socket clientSocket = this.EstablishConnection();

                int bytesRecieved = clientSocket.Receive(buffer);

                string request = Encoding.ASCII.GetString(buffer, 0, bytesRecieved);

                string[] requestParams = request.Split('\n');
                string[] requestLine = requestParams[0].Split(' ');

                int i = 1;
                Dictionary<string, string> httpHeaders = new Dictionary<string, string>();

                for (; requestParams[i].Length != 0; i++)
                {
                    string[] header = requestParams[i].Split(':');
                    httpHeaders.Add(header[0], header[1].TrimStart()); // value has leading space -> trim
                }

                // body starts at line i+1
                string requestBody = "";
                for (i++; i < requestParams.Length; i++)
                {
                    requestBody += requestParams[i];
                }


                Console.WriteLine(request);

                methodHandler.HandleMethod(requestLine[0], requestLine[1], requestBody); //method and destination
            }

        }

        
    }
}
