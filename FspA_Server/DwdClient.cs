using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.IO.Compression;


namespace FspA_Server
{
    class DwdClient
    { 
        private int a;
        private int b;
        private string adressFtp;
        private string localPath;

        private FtpWebRequest request;
        private FtpWebResponse response;
        private Stream responseStream;
        private FileStream saveStream;

        public DwdClient()
        {
            this.adressFtp = "ftp://ftp-cdc.dwd.de/pub/CDC/derived_germany/soil/daily/recent/derived_germany_soil_daily_recent_232.txt.gz";
            this.localPath = @"C:\Users\Ben\Documents\Studium Elektrotechnik\S5_Fachstudienprojekt_A\Testentpackt.txt";
        }
        ~DwdClient()
        {
            this.saveStream.Dispose();
            this.response.Close();
            this.responseStream.Close();
            //File.Delete(this.localPath);
        }

        public void setAdressFtp(string adressFtp)
        {
            this.adressFtp = adressFtp;
        }

        public string getAdressFtp()
        {
            return this.adressFtp;
        }

        public void setLocalPath(string localPath)
        {
            this.localPath = localPath;
        }

        public string getLocalPath()
        {
            return this.localPath;
        }

        public void connectToFtp()
        {
            this.request = (FtpWebRequest)WebRequest.Create(adressFtp);
            this.request.Method = WebRequestMethods.Ftp.DownloadFile;
            //Use anonymous logon
            this.request.Credentials = new NetworkCredential("anonymous", "anonymous");
        }

        public void getResponseFtp()
        {
            this.response = (FtpWebResponse)request.GetResponse();

            Console.WriteLine("Download Complete, status {0}", response.StatusDescription);

            this.responseStream = this.response.GetResponseStream();
        }

        public void decompressAndSave()
        {
            //Erzeugen einer neuen Datei im Pfad: localPath
            this.saveStream = new FileStream(this.localPath, FileMode.Create);
            GZipStream decompressionStream = new GZipStream(this.responseStream, CompressionMode.Decompress);
            decompressionStream.CopyTo(saveStream);

            decompressionStream.Close();
        }
    }
}
