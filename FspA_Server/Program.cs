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
            this.adressFtp = " ";
            this.localPath = " ";
        }

        static void Main(string[] args)
        {
            Program prog = new Program();
            Console.WriteLine("Geben Sie den gewünschten Ftp-Adresse an:");
            prog.adressFtp = @Console.ReadLine();
            Console.WriteLine("Geben Sie den gewünschten Speicherpfad an:");
            prog.localPath = @Console.ReadLine();
            Console.WriteLine("Ftp: {0}\n Pfad: {1}", prog.adressFtp, prog.localPath);
            Console.ReadLine();
        }
    }
}
