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
    /// Die Klasse State Object mit Datenfeldern
    /// </summary>
    public class StateObject
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }

    /// <summary>
    /// Die Klasse Data Server mit Datenfeldern
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
        /// Adressinforamtionen für Internethost
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
        /// Die Methode FreeTcpPort() sucht nach einem freinen Port zur Verwendung als Server-Port.
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
            /* Data buffer for input Data*/
            byte[] bytes = new Byte[1024];

            //Set the localendpoint for the Socket from the HostName.
            this.ipHostInfo = Dns.Resolve(Dns.GetHostName());
            //this.ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            /*Debug Function to display the Host Name as string*/
            Console.WriteLine("Host: {0}", Dns.GetHostName());

            /*Define the IP-Adress*/
            this.ipAddress = ipHostInfo.AddressList[0];

            //FreeTcpPort() to search for a free Port
            port = FreeTcpPort();

            this.localEndPoint = new IPEndPoint(ipAddress,port);

            //Debug Function to display the Port Number
            Console.WriteLine("Portnummer: {0}", localEndPoint.Port);

            //Debug Function for Server IP Adress
            Console.WriteLine("IP: {0}", ipAddress);
           
            // Create a TCP/IP socket.
            Socket listener = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                while (true)
                {
                    // Set the event to nonsignaled state.
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.
                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);

                    // Wait until a connection is made before continuing.
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
            // Signal the main thread to continue.
            allDone.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ar"></param>
        public void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket. 
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.
                state.sb.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read 
                // more data.
                content = state.sb.ToString();
                if (content.IndexOf("<EOF>") > -1)
                {
                    /*All the data has been read from the 
                     client. To be shure that the Data have the right encoding display it on the console.*/
                    Console.WriteLine("Read {0} bytes from socket. \n Data : {1}", content.Length, content);
                    // Start XmlHandler to create XML data for transmission.
                    XmlHandler xml = new XmlHandler();
                    //Debug function to display the incoming string without eof tag.
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
                    // recieve more data
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
            //convert the string into byte data with ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            //beginn to transmit data to the client.
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

                // Complete sending the data to the remote device.
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