using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame.http
{
    public class Response
    {
        public static string CreateResponse(string responseCode, string responseText, string responseBody, string responseType)
        {
            string response =
                "HTTP/1.1" + " " + responseCode + " " + responseText + "\r\n" +
                "Date: " + DateTime.Now + "\r\n" +
                "Content-Length: " + responseBody.Length + "\r\n" +
                "Content-Type: " + responseType + "\r\n" +
                "Connection: Closed" + "\r\n" +
                "\r\n" +
                responseBody;

            return response;
        }
    }
}
