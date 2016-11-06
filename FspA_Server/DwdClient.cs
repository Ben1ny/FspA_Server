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
            /*ftp://ftp-cdc.dwd.de/pub/CDC/observations_germany/climate/hourly/air_temperature/recent/stundenwerte_TU_00232_akt.zip*/
            this.adressFtp = "ftp://ftp-cdc.dwd.de/pub/CDC/derived_germany/soil/daily/recent/derived_germany_soil_daily_recent_232.txt.gz";
            //this.localPath = @"C:\Users\Ben\Documents\Studium Elektrotechnik\S5_Fachstudienprojekt_A\Testdata.txt";
            this.createPath = @"C:\ATFolder";
            this.dataName = "Testdata.txt";

            /*Login Dwd Grundversorger:
            User: gds26798
            Passwort: IOrbkMZj
            Server: ftp://ftp-outgoing2.dwd.de

            Connection URL: ftp://gds26798:IOrbkMZj@ftp-outgoing2.dwd.de
*/
        }

        //Destructor
        ~DwdClient()
        {
            this.saveStream.Dispose();
            this.response.Close();
            this.responseStream.Close();
            //Löscht Datei aus Lokalempfad und Ordner!!!
            // Noch nicht ganz ausgereift, Fehler bei löschen wenn zuvor eigenes Verzeichnis angegeben wird.
           /* File.Delete(this.localPath);
            Directory.Delete(this.createPath);*/
        }
        /*Anlegen der Ftp-Adresse um die Aktuellen Daten zu holen.
          Ftp Adresse + Dateiname + Dateiendung*/
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

        /*Funktion zum einlesen des lokalen Speicherortes für die Datei des Dwd.
          Dateipfad ohne Dateiname*/
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

        /*Herstellen einer Verbindung zum Ftp Server, anhand der eingegebenen Adresse.
          alternativ kann ein Passwort und Nutzername angegeben werden -> "anonymous"*/
        public void connectToFtp()
        {
            this.request = (FtpWebRequest)WebRequest.Create(adressFtp);
            this.request.Method = WebRequestMethods.Ftp.DownloadFile;
            //Use anonymous logon
            this.request.Credentials = new NetworkCredential("anonymous", "anonymous");
        }

        /*Entgegennahme der Ftpserver Antwort. Wird zwischengespeichert in einem responseStream Objekt*/
        public void getResponseFtp()
        {
            this.response = (FtpWebResponse)request.GetResponse();
           // Console.WriteLine("Download Complete, status {0}", response.StatusDescription);
            this.responseStream = this.response.GetResponseStream();
        }

        /*Anlegen eines lokalen neune Files im Speicherort localPath. Anschließend wird das responseStream Objekt mit hilfe eines GZipStream Objektes dekomprimiert
         * und im lokalen Speicherort abgespeichert.*/
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
                String[] lines = System.IO.File.ReadAllLines(@"C:\\ATFolder\\Testdata.txt"); //
                //https://msdn.microsoft.com/de-de/library/ezwyzy7b.aspx
                //https://msdn.microsoft.com/de-de/library/2c7h58e5(v=vs.110).aspx
               
                Console.WriteLine("{0} \n {1}", lines[0], lines[lines.Length - 1]); //Konsole ist die Kommandozeile als Objekt
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
