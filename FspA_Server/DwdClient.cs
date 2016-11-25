
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
using HtmlAgilityPack;
using System.Text.RegularExpressions;

/*Login Dwd Grundversorger:
            User: gds26798
            Passwort: IOrbkMZj
            Server: ftp://ftp-outgoing2.dwd.de
            Connection URL: ftp://gds26798:IOrbkMZj@ftp-outgoing2.dwd.de*/


namespace FspA_Server
{
    /// <summary>
    /// Die Klasse DwdClient erzeugt lokal einen Ordner und Speichert die aktuellen Wetterdaten vom Deutschen Wetterdienst
    /// </summary>
    class DwdClient
    { 
        /// <summary>
        /// Download-Adresse vom DWD-Server
        /// </summary>
        private string adressFtp;
        /// <summary>
        /// Speicheradresse inklusive Dateiname
        /// </summary>
        private string localPath;
        /// <summary>
        /// Lokaler Speicherort 
        /// </summary>
        private string createPath;
        /// <summary>
        /// Name der Wetterdatendatei inklusive Endung (.txt)
        /// </summary>
        private string dataName;

        //weitere Objekte
        private FtpWebRequest request;
        private FtpWebResponse response;
        private Stream responseStream;
        private FileStream saveStream;
        private FileStream cacheStream;

        /// <summary>
        /// Konstruktor der Klasse DwdClient()
        /// </summary>
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

        
        /// <summary>
        /// Destruktor der Klasse DwdClient()
        /// </summary>
        ~DwdClient()
        {
            this.saveStream.Dispose();
            this.response.Close();
            this.responseStream.Close();
            //Löscht Datei aus Lokalempfad und Ordner!!!
            // Noch nicht ganz ausgereift, Fehler bei löschen wenn zuvor eigenes Verzeichnis angegeben wird.
            /*File.Delete(this.localPath);
            Directory.Delete(this.createPath);*/
        }

        /// <summary>
        /// Die Funktion getHtmlAderssFtp ruft das aktuelle Datum ab, um die aktuellen Wetterdaten vom Ftp-Server des DWD zu holen, da diese stündlich aktuallisiert werden die Daten der 
        /// vorherigen Stunde geholt.Pro Stunde gibt es immer zwei Datenpakete 1. um X:14 und 2. um X:44 diese dienen zur Weiterverarbeitung in das XML Format.
        ///</summary>
        private void getHtmlAdressFtp()
        {

            DateTime localDate = DateTime.Now;
            string localHour;
            string helpHour;
            string localminute;

            if(localDate.Minute <= 14 || localDate.Minute > 44)
            {
                if ((localDate.Hour - 2) < 10)
                {
                    helpHour = (localDate.Hour - 2).ToString();
                    localHour = helpHour.Insert(0, "0");
                    localminute = "44";
                }
                else
                {
                    localHour = ((localDate.Hour) - 2).ToString();
                    localminute = "44";
                }
            }
            else
            {
                if((localDate.Hour - 1) < 10)
                {
                    helpHour = (localDate.Hour - 1).ToString();
                    localHour = helpHour.Insert(0, "0");
                    localminute = "14";
                }
                else
                {
                    localHour = (localDate.Hour - 1).ToString();
                    localminute = "14";
                }     
            }
            this.adressFtp = "ftp://ftp-outgoing2.dwd.de/gds/specials/observations/tables/germany/SXDL99_DWAV_" + localDate.Year.ToString() + localDate.Month.ToString() + localDate.Day.ToString() + "_" + localHour + localminute +"_U_HTML";
        }

        /// <summary>
        /// Die Methode setAdressFtp(string) dient zum Erzeugen des Datenlinks per Konsoleneingabe.
        /// Bei leerem String, wird die Standardadresse verwendet.
        /// Aufbau: Ftp Adresse + Dateiname + Dateiendung
        /// </summary>
        /// <param name="adressFtp"></param>
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

        /// <summary>
        /// Übergibt die Wetterdaten-Adresse 
        /// </summary>
        /// <returns>Liefert den Link für die Wetterdaten</returns>
        public string getAdressFtp()
        {
            return this.adressFtp;
        }

        /// <summary>
        /// Funktion zum Einlesen des lokalen Speicherortes für die Wetterdaten des DWD.
        /// Der Dateipfad wird ohne den Dateinamen angegeben.
        /// </summary>
        /// <param name="localPath"></param>
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

        /// <summary>
        /// Die Methode übergibt den Speicherpfad der Wetterdaten.
        /// </summary>
        /// <returns>Liefert den Speicherpfad der Wetterdaten.</returns>
        public string getLocalPath()
        {
            return this.localPath;
        }

        /*Herstellen einer Verbindung zum Ftp Server, anhand der eingegebenen Adresse.
          alternativ kann ein Passwort und Nutzername angegeben werden -> "anonymous"*/
        
        /// <summary>
        /// Die Methode connectToFtp() erstellt anhand der WebRequest.Create() Methode eine Verbindung mit einem Ftp-Server
        /// für einen Daten Download.
        /// Über die Klasse-NetworkCredential werden die Zugriffsrechte und Zugriffsdaten für den Ftp-Server festgelegt.
        /// </summary>
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

        /// <summary>
        /// Die Methode getResponseFtp() nimmt den verwaltet den Datastream der mithilfe der FtpWebResponse-Klasse entgegengenommen wird.
        /// </summary>
        public void getResponseFtp()
        {
            this.response = (FtpWebResponse)request.GetResponse();
           // Console.WriteLine("Download Complete, status {0}", response.StatusDescription);
            this.responseStream = this.response.GetResponseStream();
        }

        /*Anlegen eines lokalen neune Files im Speicherort localPath. Anschließend wird das responseStream Objekt mit hilfe eines GZipStream Objektes dekomprimiert
         * und im lokalen Speicherort abgespeichert.*/

        /// <summary>
        /// Die Methode decompressAndSave() erstellt durch Verwendung der FileStream-Klasse zwei Dateien eine .html und eine .txt Datei.
        /// Die .html Wetterdatei dient der Überprüfung auf Richtigkeit der Werte und die .txt Wetterdatei wird verwendet um aus ihr die Standort bezogene 
        /// Wetter XML-Datei zu erstellen.
        /// Die .html Datei wird mit der Regex-Klasse von HTML Format in das UTF8 Format umgewandelt.
        /// </summary>
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
