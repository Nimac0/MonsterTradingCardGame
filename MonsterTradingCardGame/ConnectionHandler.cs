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
    internal class ConnectionHandler
    {
        public void HandleConnection() //TODO rename 
        {
            MethodHandler methodHandler = new MethodHandler();
            Socket serverSocket = new Socket(AddressFamily.InterNetwork,
                                            SocketType.Stream,
                                            ProtocolType.Tcp);
            Console.WriteLine("socket opened");

            serverSocket.Bind(new IPEndPoint(IPAddress.Loopback, 10001));

            while (true)
            {
                serverSocket.Listen(6);
                Console.WriteLine("waiting for connections...");
                Socket clientSocket = serverSocket.Accept();
                Console.WriteLine("after waiting for connections...");

                var childSocketThread = new Thread(new ThreadStart(() => //TODO maybe make own function
                {
                    Console.WriteLine("thread started");
                    byte[] buffer = new byte[1000];

                    int bytesRecieved = clientSocket.Receive(buffer);

                    string request = Encoding.ASCII.GetString(buffer, 0, bytesRecieved);
                    string[] requestParams = request.Split('\n');
                    string[] requestLine = requestParams[0].Split(' ');

                    int i = 1;
                    Dictionary<string, string> httpHeaders = new Dictionary<string, string>();

                    for (; !requestParams[i].Equals("\r"); i++)
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
                    string authToken = "";
                    if (httpHeaders.ContainsKey("Authorization"))
                    {
                        authToken = httpHeaders["Authorization"];
                    }

                    string response;
                    try
                    {
                        response = methodHandler.HandleMethod(requestLine[0], requestLine[1], requestBody, authToken); //method and destination
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("EXCEPTION: " + e);
                        response = Response.CreateResponse("500", "Internal Server Error", "", "application/json");
                    }
                    Console.WriteLine(response);
                    clientSocket.Send(Encoding.ASCII.GetBytes(response));
                    clientSocket.Close();
                    Console.WriteLine("socket closed");
                }));
                childSocketThread.Start();
            }
                

        }

        
    }
}
