using System;
using System.IO;
using System.Net;

namespace BingWallpaper
{
    public class WebClientEx : WebClient
    {
        public WebClientEx() : this(60 * 1000)
        {
        }

        public WebClientEx(int timeout)
        {
            Timeout = timeout;
        }

        public int Timeout { get; set; }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);

            request.Timeout = Timeout;

            return request;
        }

        public MemoryStream DownloadDataStream(string address)
        {
            byte[] buffer = DownloadData(address);

            return new MemoryStream(buffer);
        }

        public MemoryStream DownloadDataStream(Uri address)
        {
            byte[] buffer = DownloadData(address);

            return new MemoryStream(buffer);
        }
    }
}