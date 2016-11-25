using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

namespace FspA_Server
{

    /// <summary>
    /// Die Klasse XML Handler mit Datenfeldern
    /// </summary>
    class XmlHandler
    {

        /// <summary>
        /// Variable für den Ort (1)
        /// </summary>
        private String location;
        /// <summary>
        /// Variable für die Temperatur
        /// </summary>
        private String tempcache;
        /// <summary>
        /// Variable für die Luftfeuchte
        /// </summary>
        private String humiditycache;
        /// <summary>
        /// Variable für den Ort (2)
        /// </summary>
        private String locationcache;
        /// <summary>
        /// Variable für das Datum
        /// </summary>
        private String datecache;
        /// <summary>
        /// Variable für die Uhrzeit
        /// </summary>
        private String timecache;
        /// <summary>
        /// Variable für das Copyright
        /// </summary>
        private String copyright;


        /// <summary>
        /// Methode zum Durchsuchen der Datei nach Ortsnamen
        /// </summary>
        /// <param name="locationRequest"></param>
        /// <returns></returns>
        public bool getLocation(string locationRequest)
        {
            location = locationRequest;
            bool yesno = false;
            String[] lines = System.IO.File.ReadAllLines(@"C:\\StudyProjectFolder\\Testdata.txt");
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Equals(location))
                {
                    yesno = true;
                    break;
                }
            }
            return yesno;
        }

        /// <summary>
        /// Methode die die DWD-Datei nach den festgelegten Parametern durchsucht
        /// </summary>
        public void openFile()
        {
            try
            {   // open the text file using a stream reader
                using (StreamReader sr = new StreamReader(@"C:\StudyProjectFolder\Testdata.txt"))
                {
                    // read the stream to a string, and write the string to the console
                    String[] lines = System.IO.File.ReadAllLines(@"C:\\StudyProjectFolder\\Testdata.txt"); 

                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (i == 2)
                        {
                            datecache = lines[i].Substring(lines[i].Length - 22, 10);
                            timecache = lines[i].Substring(lines[i].Length - 10, 5);
                            break;
                        }
                    }

                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (lines[i].Equals(location))
                        {
                            locationcache = lines[i];
                            tempcache = lines[i + 3];
                            humiditycache = lines[i + 6];
                            copyright = lines[lines.Length - 1];
                            break;
                        }
                    }
                    
                    Console.WriteLine("Ort: {0}\n Temperatur: {1}\n Luftfeuchte: {2}", location, tempcache, humiditycache);
                     
                    Console.WriteLine("\nCopei: {0}", lines[lines.Length - 1]); //Konsole ist die Kommandozeile als Objekt

                    XmlTextWriter write = new XmlTextWriter(@"c:\StudyProjectFolder\Test.xml", Encoding.UTF8);
                    write.Formatting = Formatting.Indented;
                    write.WriteStartDocument();
                    write.WriteStartElement("DWD-Daten");
                    write.WriteElementString("ort", locationcache);
                    write.WriteElementString("datum", datecache);
                    write.WriteElementString("uhrzeit", timecache);
                    write.WriteElementString("temperatur", tempcache);
                    write.WriteElementString("luftfeuchte", humiditycache);
                    write.WriteElementString("copyright", copyright);
                    write.WriteEndElement();
                    write.WriteEndDocument();
                    write.Close();

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
