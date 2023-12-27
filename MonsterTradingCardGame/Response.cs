using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame
{
    internal class Response
    {
        private string _responseCode;
        private string _responseText;
        private string _responseBody;
        private string _responseType;

        public Response(string responseCode, string responseText, string responseBody, string responseType)
        {
            this._responseCode = responseCode;
            this._responseText = responseText;
            this._responseBody = responseBody;
            this._responseType = responseType;
        }

        public string CreateResponse()
        {
            string response =
                "HTTP/1.1" + " " + this._responseCode + " " + this._responseText + "\r\n" +
                "Date: " + DateTime.Now + "\r\n" +
                "Content-Length: " + this._responseBody.Length + "\r\n" +
                "Content-Type: " + this._responseType + "\r\n" +
                "Connection: Closed" + "\r\n" +
                "\r\n" +
                this._responseBody;

            return response;
        }
    }
}
