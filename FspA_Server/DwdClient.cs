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

            Console.WriteLine("Download Complete, status {0}", response.StatusDescription);
        }
    }
}
