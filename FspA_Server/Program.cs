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

            dwdcl.openFile(); //Datei als Stream öffnen
            dwdcl = null;
        }


        static void Main(string[] args)
        {
            Program prog = new Program();
            
            Console.WriteLine("Geben Sie den gewünschten Ftp-Adresse an:");
            prog.adressFtp = @Console.ReadLine();
            Console.WriteLine("Geben Sie den gewünschten Speicherpfad an:");
            prog.localPath = @Console.ReadLine();
            
            prog.getDataDwD();
            Console.WriteLine("Ftp: {0}\n Pfad: {1}", prog.adressFtp, prog.localPath);
            Console.ReadLine();
            /*GC.Collect();
            GC.WaitForPendingFinalizers();*/
        }
    }
}
