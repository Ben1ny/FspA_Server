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
        private string adressFtp;
        private string localPath;
        private string createPath;
        private string dataName;

        private FtpWebRequest request;
        private FtpWebResponse response;
        private Stream responseStream;
        private FileStream saveStream;

        public DwdClient()
        {
            this.adressFtp = "ftp://ftp-cdc.dwd.de/pub/CDC/derived_germany/soil/daily/recent/derived_germany_soil_daily_recent_232.txt.gz";
            //this.localPath = @"C:\Users\Ben\Documents\Studium Elektrotechnik\S5_Fachstudienprojekt_A\Testdata.txt";
            this.createPath = @"C:\ATFolder";
            this.dataName = "Testdata.txt";
        }
        ~DwdClient()
        {
            this.saveStream.Dispose();
            this.response.Close();
            this.responseStream.Close();
            //Löscht Datei aus Lokalempfad und Ordner!!!
           /* File.Delete(this.localPath);
            Directory.Delete(this.createPath);*/
        }

        public void setAdressFtp(string adressFtp)
        {
            if(String.IsNullOrEmpty(adressFtp))
            {
                return;
            }
            else
            {
               this.adressFtp = adressFtp;
            }
            
        }

        public string getAdressFtp()
        {
            return this.adressFtp;
        }

        public void setLocalPath(string localPath)
        {
            if(String.IsNullOrEmpty(localPath))
            {
                System.IO.Directory.CreateDirectory(this.createPath);
                this.localPath = createPath + @"\" + dataName;
            }
            else
            {
                this.localPath = localPath + @"\" + dataName;
            }
            
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
           // Console.WriteLine("Download Complete, status {0}", response.StatusDescription);
            this.responseStream = this.response.GetResponseStream();
        }

        public void decompressAndSave()
        {
            //Erzeugen einer neuen Datei im Pfad: localPath
            this.saveStream = new FileStream(this.localPath, FileMode.Create);
            GZipStream decompressionStream = new GZipStream(this.responseStream, CompressionMode.Decompress);
            decompressionStream.CopyTo(saveStream);

            decompressionStream.Close();

            Console.WriteLine("Download Complete, status {0}", response.StatusDescription);
        }
    }
}
