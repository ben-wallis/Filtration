using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Filtration.Services
{
    internal interface IHTTPService
    {
        Task<string> GetContentAsync(string url);
    }

    internal class HTTPService : IHTTPService
    {
        public async Task<string> GetContentAsync(string url)
        {
            var urlUri = new Uri(url);

            var request = WebRequest.Create(urlUri);

            var response = await request.GetResponseAsync();
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
