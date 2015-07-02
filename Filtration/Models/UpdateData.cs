using System;
using System.Text.RegularExpressions;

namespace Filtration.Models
{
    [Serializable]
    public class UpdateData
    {
        private string _releaseNotes;

        public string DownloadUrl { get; set; }
        public decimal CurrentVersion { get; set; }
        public DateTime ReleaseDate { get; set; }

        public string ReleaseNotes
        {
            get { return _releaseNotes; }
            set
            {
                var r = new Regex("(?<!\r)\n");
                _releaseNotes = r.Replace(value, "\r\n");
            }
        }
    }
}
