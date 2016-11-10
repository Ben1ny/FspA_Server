using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

namespace FspA_Server
{
    class XmlHandler
    {
        private String location;
        private String date;
        private String time;
        private String tempcache;
        private String humiditycache;
        private String locationcache;
        private String datecache;
        private String timecache;
        private String copyright;
        private int temp;
        private int humidity;
        private int textLength;
  
        public void openFile()
        {
            try
            {   // Open the text file using a stream reader.
                using (StreamReader sr = new StreamReader(@"C:\ATFolder\Testdata.txt"))
                {
                    // Read the stream to a string, and write the string to the console. //Hierher
                    //https://msdn.microsoft.com/de-de/library/db5x7c0d(v=vs.110).aspx
                    String[] lines = System.IO.File.ReadAllLines(@"C:\\ATFolder\\Testdata.txt"); //
                    textLength = lines.Length;
                    Console.WriteLine(textLength);
                                     
                    location = Console.ReadLine();
                    /*
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
                    */

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

                    //https://www.youtube.com/watch?v=iSUxvnppFJA
                    //http://stackoverflow.com/questions/9382846/how-to-insert-c-variables-in-xml

                    XmlTextWriter write = new XmlTextWriter(@"c:\ATFolder\Test.xml", Encoding.UTF8);
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

                    /*

                    write.WriteStartDocument();
                    write.Formatting = Formatting.Indented;

                    write.WriteStartElement("temp");
                    write.WriteValue(tempcache);
                    write.WriteEndElement();
                    
                    //write.WriteEndElement();
                    //write.WriteStartElement("hum");
                    //write.WriteStartElement("hum");
                    //write.WriteValue(humiditycache);
                    //write.WriteEndElement();

                    write.WriteEndDocument();
                    write.Close();
                    */

                    /*public static XDocument Parse(
                        string text
                    )
                    string xmlstr = @"<?xml version=""1.0""?>
                    <!-- comment at the root level -->
                    <Root>
                    <temp>Content</temp>
                    <hum>Content</hum>
                    </Root>";

                    XDocument doc = XDocument.Parse(xmlstr);
                    Console.WriteLine(doc);
            */
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

/*
<xs:element name="ort" type="xs:string"/>
<xs:element name="datum" type="xs:string"/> <!--20161101-->
<xs:element name="uhrzeit" type="xs:string"/> <!--2000-->
<xs:element name="temperatur" type="xs:byte"/> <!--Celsius-->
<xs:element name="luftfeuchte" type="xs:byte"/> <!--Prozentwert-->

    Copyright Dwd
*/
