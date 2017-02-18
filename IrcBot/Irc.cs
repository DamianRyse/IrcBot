using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace IrcBot
{
    class Irc
    {
        // Eigenschaften der Klasse
        public string Hostname { get; }
        public int Port { get; }
        public string Nickname { get; }
        public string Realname { get; }
        public bool Connected { get; private set; }
        public string ServerAdress { get; set; }

        // Benötigte Klassen um eine Verbindung zum Server aufzubauen
        TcpClient tcpClient;
        StreamReader inputStream;
        StreamWriter outputStream;

        // Queue-Stapel um mehrere Chat-Nachrichten nacheinander auszugeben.
        Queue<string> MessageQueue = new Queue<string>();
        bool QueueIsRunning = false;
        int QueueTimer = 800;

        // Zufälliges Passwort für die Anmeldung am IRC-Server. Passwort wird beim Initialisieren der Klasse generiert.
        private string password;

        public Irc()
        {

        }

        public Irc(string hostname, int port, string nickname, string realname, string defaultChannel)
        {
            Hostname = hostname;
            Port = port;
            Nickname = nickname;
            Realname = realname;
        }

        public Irc(ClientConfiguration clientConfig)
        {
            Hostname = clientConfig.ServerDetails.ServerURL;
            Port = clientConfig.ServerDetails.ServerPort;
            Nickname = clientConfig.ServerDetails.Nick;
            Realname = clientConfig.ServerDetails.User;
        }

        public void Connect()
        {
            try
            {
                // Verbindung zum Server herstellen.
                tcpClient = new TcpClient(Hostname, Port);

                // Lese- und Schreibstream initialisieren.
                inputStream = new StreamReader(tcpClient.GetStream());
                outputStream = new StreamWriter(tcpClient.GetStream());

                // Generiere Zufallspasswort für halbwegs gesicherte übertragungen.
                password = GeneratePassword(30);

                // Registrierung nach Empfehlung RFC 1459
                sendData("PASS " + password);
                sendData("NICK " + Nickname);
                sendData("USER " + Nickname + " 8 * :" + Realname);

                Connected = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Connected = false;
            }
        }

        public string readData()
        {
            try
            {
                return inputStream.ReadLine();
            }
            catch (Exception ex)
            {
                return "COULD NOT READ DATA: " + ex.Message;
            }
        }

        public void sendData(string Data)
        {
            try
            {
                outputStream.WriteLine(Data);
                outputStream.Flush();
                Console.WriteLine("< " + Data);
            }
            catch (Exception ex)
            {
                Console.WriteLine("COULD NOT SEND DATA: " + ex.Message);
            }
        }

        public void joinRoom(string Raumname)
        {
            // Prüft, ob Raumname dem geforderten Format entspricht.
            // Raumname darf nicht größer als 200 Zeichen sein. RFC 1459-Standart
            if (Regex.IsMatch(Raumname, "#([a-zA-Z0-9]+)") && Raumname.Length < 201)
                sendData("JOIN " + Raumname);

        }

        public void ChatMessage(string Nachricht, string Ziel)
        {
            // PRIVMSG #Raum :Die Chatnachricht
            string Message = "PRIVMSG " + Ziel + " :" + Nachricht;
            MessageQueue.Enqueue(Message);

            if (!QueueIsRunning)
                sendChatMessage();
                

        }

        private void sendChatMessage()
        {
            QueueIsRunning = true;
            foreach(string Zeile in MessageQueue)
            {
                sendData(Zeile);
                System.Threading.Thread.Sleep(QueueTimer);
            }
            MessageQueue.Clear();
            QueueIsRunning = false;
        }

        /// <summary>
        /// Generiert ein zufälliges Passwort.
        /// </summary>
        /// <param name="Length">Die Anzahl der Stellen, die das Passwort haben soll.</param>
        /// <returns></returns>
        private string GeneratePassword(Int64 Length)
        {
            StringBuilder sb = new StringBuilder();
            Random rnd = new Random();
            for (int i = 0; i < Length; i++)
            {
                sb.Append(Convert.ToChar((byte)rnd.Next(65,130))).ToString();
            }
            return sb.ToString();
        }

    }
}