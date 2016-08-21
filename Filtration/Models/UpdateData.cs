using System;
using System.Text.RegularExpressions;

namespace Filtration.Models
{
    [Serializable]
    public class UpdateData
    {
        private string _releaseNotes;

        public string DownloadUrl { get; set; }
        public int LatestVersionMajorPart { get; set; }
        public int LatestVersionMinorPart { get; set; }
        public DateTime ReleaseDate { get; set; }
        public DateTime StaticDataUpdatedDate { get; set; }

        public string ReleaseNotes
        {
            get { return _releaseNotes; }
            set
            {
                var r = new Regex("(?<!\r)\n");
                _releaseNotes = r.Replace(value, "\r\n");
            }
        }

        // Not de-serialized from XML update file
        public int CurrentVersionMajorPart { get; set; }
        public int CurrentVersionMinorPart { get; set; }
        public bool UpdateAvailable { get; set; }
    }
}
