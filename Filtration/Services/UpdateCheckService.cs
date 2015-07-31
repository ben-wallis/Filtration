using System.IO;
using System.Xml.Serialization;
using Filtration.Models;

namespace Filtration.Services
{
    internal interface IUpdateCheckService
    {
        UpdateData GetUpdateData();
    }

    internal class UpdateCheckService : IUpdateCheckService
    {
        private readonly IHTTPService _httpService;
        private const string UpdateDataUrl = "http://ben-wallis.github.io/Filtration/filtration_version.xml";

        public UpdateCheckService(IHTTPService httpService)
        {
            _httpService = httpService;
        }

        public UpdateData GetUpdateData()
        {
            var updateXml = _httpService.GetContent(UpdateDataUrl);
            return (DeserializeUpdateData(updateXml));
        }

        public UpdateData DeserializeUpdateData(string updateDataString)
        {
            var serializer = new XmlSerializer(typeof(UpdateData));
            object result;

            using (TextReader reader = new StringReader(updateDataString))
            {
                result = serializer.Deserialize(reader);
            }

            return result as UpdateData;
        }

    }
}
