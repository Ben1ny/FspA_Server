using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace FspA_Server
{

    /// <summary>
    /// Die Klasse State Object dient als Status für die eingehenden Client Anfragen
    /// </summary>
    public class StateObject
    {
        /// <summary>
        /// Client Socket
        /// </summary>
        public Socket workSocket = null;
        /// <summary>
        /// Größe des Empfangsspeichers
        /// </summary>
        public const int BufferSize = 1024;
        /// <summary>
        /// Empfangsspeicher
        /// </summary>
        public byte[] buffer = new byte[BufferSize];
        /// <summary>
        /// eingehender Daten String
        /// </summary>
        public StringBuilder sb = new StringBuilder();
    }

    /// <summary>
    /// Die Klasse Data Server erzeugt die Server Verbindung und bearbeitet eingehende Client Anfragen
    /// </summary>
    public class DataServer
    {
        /// <summary>
        /// Variable zur Erzeugung der Single Instanz
        /// </summary>
        private static DataServer single_Instance = null;

        /// <summary>
        /// Variable zur Festlegung des Port
        /// </summary>
        private int port;
        /// <summary>
        /// Adressinformationen für Internethost
        /// </summary>
        private IPHostEntry ipHostInfo;
        /// <summary>
        /// IP-Adresse 
        /// </summary>
        private IPAddress ipAddress;
        /// <summary>
        /// Netzwerkendpunkt mit IP-Adresse und Port
        /// </summary>
        private IPEndPoint localEndPoint;
        /// <summary>
        /// Ereignissignal
        /// </summary>
        public static ManualResetEvent allDone;

        /// <summary>
        /// Konstruktor der Klasse DataServer()
        /// </summary>
        public DataServer()
        { 
            allDone = new ManualResetEvent(false);
        }

        /// <summary>
        /// Die Methode Ceck_Instance prüft ob ein DataServer Objekt besteht.
        /// Besteht kein Objekt, wir ein neues angelegt, ansonsten wird das bestehende zurückgegeben.
        /// </summary>
        /// <returns>single_Instance</returns>
        public static DataServer Ceck_Instance()
        {
            if(single_Instance == null)
            {
                single_Instance = new DataServer();
            }
            return single_Instance;
        }

        /// <summary>
        /// Die Methode FreeTcpPort() sucht nach einem freien Port zur Verwendung als Server-Port.
        /// </summary>
        /// <returns>Liefert die freie Portnummer zurück.</returns>
        static int FreeTcpPort()
        {
            TcpListener lisn = new TcpListener(IPAddress.Loopback, 0);
            lisn.Start();
            int port = ((IPEndPoint)lisn.LocalEndpoint).Port;
            lisn.Stop();
            return port;
        }

        /// <summary>
        /// Die Methode StartListening() startet die asynchrone Verbindung als Server. Sobald eine Verbindung mit einem Client aufgebaut wird, wird diese 
        /// abgearbeitet und anschließend geschlossen. Erst dann wird die nächste Anfrage bearbeitet.
        /// </summary>
        public void StartListening()
        {
            /* Data Speicher für eingehende Data*/
            byte[] bytes = new Byte[1024];

            // Festlegen des lokalen Endpunktes für das Socket anhand des Hostnamen
            this.ipHostInfo = Dns.Resolve(Dns.GetHostName());
            //this.ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());

            /*Debug Funktion: Host Name als String*/
            Console.WriteLine("Host: {0}", Dns.GetHostName());

            //Festlegen der IP-Adress
            this.ipAddress = ipHostInfo.AddressList[0];

            //FreeTcpPort() sucht nach einem freien Port
            port = FreeTcpPort();

            this.localEndPoint = new IPEndPoint(ipAddress,port);

            //Debug Funktion: Port Number
            Console.WriteLine("Portnummer: {0}", localEndPoint.Port);

            //Debug Funktion: Server IP Adress
            Console.WriteLine("IP: {0}", ipAddress);
           
            // Erzeugt ein TCP/IP Socket.
            Socket listener = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);

            // Bindet das Socket an den lokalen Endpunkt und wartet auf einkommende Verbindungen
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    // Setzt das Abfrage Signal zurück
                    allDone.Reset();

                    // Startet ein asynchrones Socket, um auf eine Verbindung zu warten
                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);

                    // Wartet bis eine Verbindung besteht bevor weiter gemacht wird
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }

        /// <summary>
        /// Nimmt die Anfrage eines Client entgegen und startet mit den Informationen des Socket den Empfang der Daten.
        /// </summary>
        /// <param name="ar"></param>
        public void AcceptCallback(IAsyncResult ar)
        {
            // Signal zum weitermachen
            allDone.Set();

            // Entgegennehmen des Sockets das die Asynchrone anfrage gestartet hat
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Erstellen des StateObject
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        /// <summary>
        /// Verarbeitet die eingehende Client Anfrage und gibt bei Übereinstimmung mit einer Wetterstation die Wetterdaten als XML Datei zurück an den Client.
        /// </summary>
        /// <param name="ar"></param>
        public void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            //Abrufen des state object und des Sockets von dem Asynchronen state object
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            //Lesen der Daten vom Client socket
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                // Da mehr Daten vorhanden sein könnten müssen die empfangenen gespeichert werden
                state.sb.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead));

                //Überprüfen auf <eof> Ende Abfrage, wenn nicht vorhanden empfange mehr Daten 
                content = state.sb.ToString();
                if (content.IndexOf("<EOF>") > -1)
                {
                    //Gibt die empfangenen Daten auf der Konsole aus
                    Console.WriteLine("Read {0} bytes from socket. \n Data : {1}", content.Length, content);
                    // Start XmlHandler to create XML data for transmission.
                    XmlHandler xml = new XmlHandler();
                    //Debug Funktion Eingehender String ohne Endabfrage <eof>
                    //Console.WriteLine("Länge String: {0} \nIndex <EOF>: {1}\nString nach cut: {2}", content.Length, content.IndexOf("<EOF>"), content.Substring(0, (content.IndexOf("<EOF>"))));
                    xml.getLocation(content.Substring(0, (content.IndexOf("<EOF>"))));
                    if (xml.getLocation(content.Substring(0, (content.Length - 5))) == true)
                    {
                        xml.openFile();
                        Send(handler, File.ReadAllText(@"C:\StudyProjectFolder\Test.xml"));
                    }
                    else
                    {
                        Send(handler, "Wetterstation " + content.Substring(0, (content.Length - 5)) + " konnte nicht gefunden werden.");
                    }  
                }
                else
                {
                    // Empfang weiterer Daten
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
            }
        }

        /// <summary>
        /// Die Methode Send der DataServer-Klasse sendet einen Datenstring in ASCII umgewandelt per byte[] an den Client.
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="data"></param>
        private void Send(Socket handler, String data)
        {
            //Konvertiert den String in Bytes mit ASCII Kodierung.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            //Beginnt mit der Übertragung der Daten an den Client
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        /// <summary>
        /// Signalisiert das Ende der Datenübertragung an den Client.
        /// </summary>
        /// <param name="ar"></param>
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Abrufen des Socket aus dem StateObject
                Socket handler = (Socket)ar.AsyncState;

                //Daten wurden an den Client komplett übertragen 
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    } 
}