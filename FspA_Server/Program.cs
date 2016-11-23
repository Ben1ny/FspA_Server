using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Net;



namespace FspA_Server
{
    /// <summary>
    /// Die Klasse (Haupt-)Programm dient zur Abarbeitung des Wetterservers
    /// </summary>
    class Program
    {
        /// <summary>
        /// Link zum Datenabruf vom DWD-Ftp Server
        /// </summary>
        public string adressFtp;
        /// <summary>
        /// Speicherpfad für die Wetterdaten
        /// </summary>
        private string localPath;

        public Program()
        {
            this.adressFtp = "";
            this.localPath = "";
        }

        /// <summary>
        /// Die Funktion getDataDwD() holt die Wetterdaten vom DWD Server und Speicher diese auf dem lokalen PC ab.
        /// </summary>
        public void getDataDwD()
        {
            DwdClient dwdcl = new DwdClient();
            
            dwdcl.setAdressFtp(this.adressFtp);
            dwdcl.setLocalPath(this.localPath);
            //Debug
            Console.WriteLine("Ftp: {0}\n Pfad: {1}", dwdcl.getAdressFtp(), dwdcl.getLocalPath());
            dwdcl.connectToFtp();
            dwdcl.getResponseFtp();
            dwdcl.decompressAndSave();

            //Markus Test Funktion
            /*string locationTest;
            XmlHandler xml = new XmlHandler();
            
            do
            {
                Console.WriteLine("Geben Sie die gewünschte Station ein:");
                locationTest = Console.ReadLine();
                xml.getLocation(locationTest);
            }while( xml.getLocation(locationTest) == false);
            
            xml.openFile();*/

            dwdcl = null;
        }


        /// <summary>
        /// Main Methode in welcher der Datenabruf von dem DWD-Ftp Server an den Local-PC gestartet wird und anschließend der Local-PC als Server für 
        /// andere Clients zur Verfügung steht. Diese können Wetterdaten im XML-Format per Standort-Anforderung abrufen.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Program prog = new Program();
            DataServer dats2 = new DataServer();

            /*Console.WriteLine("Geben Sie den gewünschten Ftp-Adresse an:");
            prog.adressFtp = @Console.ReadLine();
            Console.WriteLine("Geben Sie den gewünschten Speicherpfad an:");
            prog.localPath = @Console.ReadLine();*/
            
            prog.getDataDwD();

            //Benny Funktion Server
            dats2.StartListening();
            
            Console.ReadLine();
            /*GC.Collect();
            GC.WaitForPendingFinalizers();*/
        }
    }
}

/*

<xs:element name="ort" type="xs:string"/>
<xs:element name="datum" type="xs:string"/>
<xs:element name="uhrzeit" type="xs:string"/>
<xs:element name="temperatur" type="xs:decimal"/>
<xs:element name="luftfeuchte" type="xs:byte"/>
<xs:element name="copyright" type="xs:string"/>

*/
