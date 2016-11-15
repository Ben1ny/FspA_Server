
//#define GZIP
//#define ZIP
#define HTML

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Web;
//using System.Web.Http;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

/*Login Dwd Grundversorger:
            User: gds26798
            Passwort: IOrbkMZj
            Server: ftp://ftp-outgoing2.dwd.de
            Connection URL: ftp://gds26798:IOrbkMZj@ftp-outgoing2.dwd.de*/


namespace FspA_Server
{
    class DwdClient
    { 
        private string adressFtp;
        private string localPath;
        private string createPath;
        private string dataName;

        //weitere Objekte
        private FtpWebRequest request;
        private FtpWebResponse response;
        private Stream responseStream;
        private FileStream saveStream;
        private FileStream cacheStream;

        //Constructor
        public DwdClient()
        {
#if (GZIP)
            this.adressFtp = "ftp://ftp-cdc.dwd.de/pub/CDC/derived_germany/soil/daily/recent/derived_germany_soil_daily_recent_232.txt.gz";
#elif (ZIP)
            this.adressFtp = "ftp://ftp-cdc.dwd.de/pub/CDC/observations_germany/climate/hourly/air_temperature/recent/stundenwerte_TU_00232_akt.zip";
#elif (HTML)
           // this.adressFtp = "ftp://ftp-outgoing2.dwd.de/gds/specials/observations/tables/germany/SXDL99_DWAV_20161111_1514_U_HTML";
            getHtmlAdressFtp();
#endif
            this.createPath = @"C:\StudyProjectFolder";
            this.dataName = "Testdata.txt";
            getHtmlAdressFtp(); 
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

        /*Die Funktion getHtmlAderssFtp ruft das aktuelle Datum ab, um die aktuellen Wetterdaten vom Ftp-Server Grundversorger zu holen, da diese stündlich aktuallisiert werden wird 
         Stunde - 1 gemacht. Ebenfalls werden die Daten immer um die Minute 14 freigegeben, und diese Werte werden weiterverarbeitet.*/
        private void getHtmlAdressFtp()
        {
            DateTime localDate = DateTime.Now;
            string localHour;
            string helpHour;
            if(localDate.Minute <= 14)
            {
                if ((localDate.Hour - 2) < 10)
                {
                    helpHour = (localDate.Hour - 2).ToString();
                    localHour = helpHour.Insert(0, "0");
                }
                else
                {
                    localHour = ((localDate.Hour) - 2).ToString();
                }
                
            }
            else
            {
                if((localDate.Hour - 1) < 10)
                {
                    helpHour = (localDate.Hour - 1).ToString();
                    localHour = helpHour.Insert(0, "0");
                }
                else
                {
                    localHour = (localDate.Hour - 1).ToString();
                }
                  
            }
            
            
            this.adressFtp = "ftp://ftp-outgoing2.dwd.de/gds/specials/observations/tables/germany/SXDL99_DWAV_" + localDate.Year.ToString() + localDate.Month.ToString() + localDate.Day.ToString() + "_" + localHour + "14_U_HTML";
            //Debug Funktion to display the Current Adress with the Current time
            //Console.WriteLine("Aktuelle Uhrzeit html: {0}", localHour);
            Console.WriteLine(adressFtp);
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
            /*Eigene Zugangsdaten für Grundversorgerdaten*/
#if (GZIP || ZIP)
            this.request.Credentials = new NetworkCredential("anonymous", "anonymous");
#elif (HTML)
            this.request.Credentials = new NetworkCredential("gds26798", "IOrbkMZj");
#endif


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
#if (GZIP)
            this.saveStream = new FileStream(this.localPath, FileMode.Create);
            GZipStream decompressionStream = new GZipStream(this.responseStream, CompressionMode.Decompress);
            decompressionStream.CopyTo(saveStream);
            decompressionStream.Close();

#elif (ZIP)
            this.saveStream = new FileStream(@"C:\ATFolder\Test.zip", FileMode.Create);
            responseStream.CopyTo(saveStream);
            saveStream.Close();
            ZipFile.ExtractToDirectory(@"C:\ATFolder\Test.zip", @"C:\ATFolder");
#elif (HTML)
            this.cacheStream = new FileStream(@"C:\StudyProjectFolder\Testhtmlorigin.html", FileMode.Create);
            this.saveStream = new FileStream(this.localPath, FileMode.Create);
            responseStream.CopyTo(this.cacheStream);
            this.cacheStream.Close();
            this.saveStream.Close();

            String chacheString;// = new StreamReader(@"C:\ATFolder\Testhtmlorigin.html").ReadToEnd();

            HtmlDocument doc = new HtmlDocument();
            doc.Load(@"C:\StudyProjectFolder\Testhtmlorigin.html");
            chacheString = doc.DocumentNode.InnerText;

            chacheString = Regex.Replace(chacheString, @"( |\t|\r?\n)\1+", "$1");
            //chacheString = Regex.Replace(chacheString, @"\s+$", "");
            chacheString = Regex.Replace(chacheString, "&auml;", "ä");
            chacheString = Regex.Replace(chacheString, "&ouml;", "ö");
            chacheString = Regex.Replace(chacheString, "&uuml;", "ü");
            chacheString = Regex.Replace(chacheString, "&Auml;", "Ä");
            chacheString = Regex.Replace(chacheString, "&Ouml;", "Ö");
            chacheString = Regex.Replace(chacheString, "&Uuml;", "Ü");
            chacheString = Regex.Replace(chacheString, "&szlig;", "ß");
            chacheString = Regex.Replace(chacheString, "&minus;", "–");
            chacheString = Regex.Replace(chacheString, "&hellip;", "…");
            // Ersetzt &copy; durch das © gefunden auf stacoverflow.com
            chacheString = Regex.Replace(chacheString, "&copy;", "©");
           // chacheString = Regex.Replace(chacheString, )
            System.IO.File.WriteAllText(this.localPath, chacheString);

            /*Hier werden die einzelnen Children der Wetterdaten des Html Dokuments auf die gleiche Ebene gebracht wie die Parent 
             um die Daten leichter und ohne Leerzeichen in eine String reinladen zu können.*/
            String[] meanwhile = File.ReadAllLines(this.localPath);
            int help;
            for (help = 18; help < (meanwhile.Length - 19); help++)
            {
                //if(meanwhile[help].IndexOf(" ",0,1))
                meanwhile[help] = meanwhile[help].Substring(1);
            }
            File.WriteAllLines(this.localPath, meanwhile);
#endif
            Console.WriteLine("Download Complete, status {0}", response.StatusDescription);
        }
    }
}
