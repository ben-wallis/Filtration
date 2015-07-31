using System;
using System.IO;
using System.Net;

namespace Filtration.Services
{
    internal interface IHTTPService
    {
        string GetContent(string url);
    }

    internal class HTTPService : IHTTPService
    {
        public string GetContent(string url)
        {
            var urlUri = new Uri(url);

            var request = WebRequest.Create(urlUri);

            var response = request.GetResponse();
            using (var s = response.GetResponseStream())
            {
                using (var sr = new StreamReader(s))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}
