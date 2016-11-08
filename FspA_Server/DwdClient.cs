
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
using System.Web.Http;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

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
        private FileStream cacheStream;

        //Constructor
        public DwdClient()
        {
#if (GZIP)
            this.adressFtp = "ftp://ftp-cdc.dwd.de/pub/CDC/derived_germany/soil/daily/recent/derived_germany_soil_daily_recent_232.txt.gz";
#elif (ZIP)
            this.adressFtp = "ftp://ftp-cdc.dwd.de/pub/CDC/observations_germany/climate/hourly/air_temperature/recent/stundenwerte_TU_00232_akt.zip";
#elif (HTML)
            this.adressFtp = "ftp://ftp-outgoing2.dwd.de/gds/specials/observations/tables/germany/SXDL99_DWAV_20161107_2014_U_HTML";
#endif
            this.createPath = @"C:\ATFolder";
            this.dataName = "Testdata.txt";

            /*Login Dwd Grundversorger:
            User: gds26798
            Passwort: IOrbkMZj
            Server: ftp://ftp-outgoing2.dwd.de
            Connection URL: ftp://gds26798:IOrbkMZj@ftp-outgoing2.dwd.de*/
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
           /* this.saveStream = new FileStream(this.localPath, FileMode.Create);
            responseStream.CopyTo(saveStream);
            saveStream.Close();
            FileStream helpst = new FileStream(@"C:\ATFolder\Testhtml.txt", FileMode.Create);
            helpst.Close();
            TextWriter ttest = File.CreateText(@"C:\ATFolder\Testhtml.txt");
            string hallo;
            string hallo2;
            StreamReader test = new StreamReader(this.localPath);
            hallo = test.ReadToEnd();
            //HttpUtility.HtmlDecode(hallo,ttest);
            WebUtility.HtmlDecode(hallo, ttest);*/

            this.cacheStream = new FileStream(@"C:\ATFolder\Testhtmlorigin.html", FileMode.Create);
            this.saveStream = new FileStream(this.localPath, FileMode.Create);
            responseStream.CopyTo(this.cacheStream);
            this.cacheStream.Close();
            this.saveStream.Close();

            String chacheString = new StreamReader(@"C:\ATFolder\Testhtmlorigin.html").ReadToEnd();

            HtmlDocument doc = new HtmlDocument();
            doc.Load(@"C:\ATFolder\Testhtmlorigin.html");
            chacheString = doc.DocumentNode.InnerText;

            chacheString = Regex.Replace(chacheString, @"( |\t|\r?\n)\1+", "$1");
            chacheString = Regex.Replace(chacheString, "&auml;", "ä");
            chacheString = Regex.Replace(chacheString, "&ouml;", "ö");
            chacheString = Regex.Replace(chacheString, "&uuml;", "ü");
            chacheString = Regex.Replace(chacheString, "&Auml;", "Ä");
            chacheString = Regex.Replace(chacheString, "&Ouml;", "Ö");
            chacheString = Regex.Replace(chacheString, "&Uuml;", "Ü");
            chacheString = Regex.Replace(chacheString, "&szlig;", "ß");
            chacheString = Regex.Replace(chacheString, "&minus;", "–");
            chacheString = Regex.Replace(chacheString, "&hellip;", "…");
            // chacheString = Regex.Replace(chacheString, Chr(147), "&ldquo;");
            // chacheStringchacheString = Regex.Replace(chacheString, Char(132), "&bdquo;");

            System.IO.File.WriteAllText(this.localPath, chacheString);
            /*String[] teest = System.IO.File.ReadAllLines(@"C:\ATFolder\WriteLines.txt");
            Console.WriteLine("Text: {0}", teest[4]);*/
#endif
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

        /*Neue Funktion zum einlesen der einzelnen Bits einer Datei.*/
        public void convertTexttoXml()
        {
            string hallo;
            byte[] buffer2 = new byte[100];
            //FileStream test = new FileStream(this.localPath, FileMode.Open);
            StreamReader test = new StreamReader(this.localPath);
            //test.Read(buffer2, 0, 21);
            hallo = test.ReadToEnd();
            Console.WriteLine("Test: {0}", hallo);
            
            //Console.WriteLine("Test: ");
        }
    }
}
