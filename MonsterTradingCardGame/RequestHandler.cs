using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
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
            ProtocolHandler protocolHandler = new ProtocolHandler();
            byte[] buffer = new byte[256];
            Socket clientSocket = this.EstablishConnection();

            int bytesRecieved = clientSocket.Receive(buffer);

            string request = Encoding.ASCII.GetString(buffer, 0, buffer.Length);

            string method = protocolHandler.GetMethod(request);
            string destination = protocolHandler.GetDestination(request);


            Console.WriteLine(request);

        }

        public void HandleMethod(string method, string destination)
        {
            switch(method)
            {
                case "GET": this.HandleGetRequest(destination);
                    break;
                case "POST": this.HandlePostRequest(destination);
                    break;
                case "PUT": this.HandlePutRequest(destination);
                    break;
                case "DELETE": this.HandleDeleteRequest(destination);
                    break;
                default: 
                    break;
            }
        }

        public void HandleGetRequest(string destination)
        {
            switch (destination)
            {

            }
        }

        public void HandlePostRequest(string destination)
        {

        }

        public void HandlePutRequest(string destination)
        {

        }

        public void HandleDeleteRequest(string destination)
        {

        }

        public bool ValidRequest { get; set; }

        public RequestHandler()
        {
            
        }
    }
}
