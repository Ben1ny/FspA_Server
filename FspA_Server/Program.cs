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
    class Program
    {
        public string adressFtp;
        private string localPath;

        public Program()
        {
            this.adressFtp = "";
            this.localPath = "";
        }

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
            Console.WriteLine("Geben Sie die gewünschte Station ein:");
            locationTest = Console.ReadLine();
            xml.getLocation(locationTest);
            xml.openFile();*/

            dwdcl = null;
        }


        static void Main(string[] args)
        {
            Program prog = new Program();
            DataServer dats2 = new DataServer();
            

            Console.WriteLine("Geben Sie den gewünschten Ftp-Adresse an:");
            prog.adressFtp = @Console.ReadLine();
            Console.WriteLine("Geben Sie den gewünschten Speicherpfad an:");
            prog.localPath = @Console.ReadLine();
            
           /* prog.getDataDwD();
            Console.WriteLine("Ftp: {0}\n Pfad: {1}", prog.adressFtp, prog.localPath);*/
            prog.getDataDwD();

            //Benny Funktion Server
            //dats2.StartListening();
            
            Console.ReadLine();
            /*GC.Collect();
            GC.WaitForPendingFinalizers();*/
        }
    }
}

/*
<xs:element name="ort" type="xs:string"/>
<xs:element name="datum" type="xs:string"/> <!--20161101-->
<xs:element name="uhrzeit" type="xs:string"/> <!--2000-->
<xs:element name="temperatur" type="xs:byte"/> <!--Celsius-->
<xs:element name="luftfeuchte" type="xs:byte"/> <!--Prozentwert-->

    Copyright Dwd
*/
