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
        //private char array;

        //weitere Objekte
        private FtpWebRequest request;
        private FtpWebResponse response;
        private Stream responseStream;
        private FileStream saveStream;

        //Constructor
        public DwdClient()
        {
            this.adressFtp = "ftp://ftp-cdc.dwd.de/pub/CDC/derived_germany/soil/daily/recent/derived_germany_soil_daily_recent_232.txt.gz";
            //this.localPath = @"C:\Users\Ben\Documents\Studium Elektrotechnik\S5_Fachstudienprojekt_A\Testdata.txt";
            this.createPath = @"C:\ATFolder";
            this.dataName = "Testdata.txt";
        }

        //Destructor
        ~DwdClient()
        {
            this.saveStream.Dispose();
            this.response.Close();
            this.responseStream.Close();
            //Löscht Datei aus Lokalempfad und Ordner!!!
            //File.Delete(this.localPath);
            //Directory.Delete(this.createPath);
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
            saveStream.Close();

            Console.WriteLine("Download Complete, status {0}", response.StatusDescription);
        }

        public void openFile()
        {
            try
            {   // Open the text file using a stream reader.
                using (StreamReader sr = new StreamReader(@"C:\ATFolder\Testdata.txt"))
                {
                // Read the stream to a string, and write the string to the console. //Hierher
                //https://msdn.microsoft.com/de-de/library/db5x7c0d(v=vs.110).aspx
                //String[] lines = System.IO.File.ReadAllLines(@"C:\\ATFolder\\Testdata.txt"); //
                //https://msdn.microsoft.com/de-de/library/ezwyzy7b.aspx
                //https://msdn.microsoft.com/de-de/library/2c7h58e5(v=vs.110).aspx
                String line = sr.ReadLine(); //nur die 1. Zeile lesen
                                             //String line = sr.ReadToEnd();//komplette Datei lesen
                Console.WriteLine(line);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
        }
    }
}
