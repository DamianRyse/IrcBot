//////////////////////////////////////
//                                  // 
//        IRC-Bot Tutorial          //
//        für C# / .NET             //
//                                  //
//////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;

namespace IrcBot
{
    class IrcBot
    {
        Irc IrcConnection;
        string ConfigFile;


        static void Main(string[] args)
        {
            IrcBot Program = new IrcBot();
            Program.LoadArguments(args);
            Program.GetData();
        }

        private void LoadArguments(string[] args)
        {
            // Prüfen, ob das Array args mindestens ein Element enthält.
            if (args != null && args.Length > 0)
            {

                // Alle Argumente in einer Schleife auslesen und auswerten.
                foreach (string argument in args)
                {
                    // Config-Datei laden.
                    if (argument.ToLower().Substring(0, 4) == "cfg=" && argument.Length > 7)
                    {
                        Console.WriteLine("Lade Konfiguration: " + argument.Split(Convert.ToChar("="))[1]);
                        ConfigFile = argument.Split(Convert.ToChar("="))[1];
                    }
                    else
                    {
                        Console.WriteLine("Unbekanntes Argument: " + argument);
                    }
                }
            }
            else
            {
                CreateConfigFile();
            }
            
        }

        private void CreateConfigFile()
        {
            Console.Clear();
            Console.WriteLine("--- Neue Konfiguration anlegen");

            string server, nickname, realname, defaultChannel, fileName;
            int port;

            Console.Write("IRC-Server Hostname: ");
            server = Console.ReadLine();

            Console.Write("IRC-Server Port: ");
            while(!int.TryParse(Console.ReadLine(),out port) || port < 0 || port > 65535)
            {
                Console.WriteLine("Ungültige Portangabe. Bitte geben Sie einen Port von 0 bis 65535 ein.");
                Console.Write("IRC-Server Port: ");
            }

            Console.Write("Bot Nickname: ");
            while(!Regex.IsMatch(nickname = Console.ReadLine(), @"^(.[^ ,#]+)$"))
            {
                Console.WriteLine("Ungüliges Format für den Nickname. Ein Nickname darf keine Kommata, kein #-Symbol und kein Leerzeichen enthalten.");
                Console.Write("Bot Nickname: ");
            }

            Console.Write("Bot Realname: ");
            realname = Console.ReadLine();

            Console.Write("Default Channel: ");
            while (!Regex.IsMatch(defaultChannel = Console.ReadLine(), "^#(.[^, ]+)$"))
            {
                Console.WriteLine("Ungültiger Channelname. Ein Channel beginnt mit einem #-Symbol, darf keine Kommata und keine Leerzeichen enthalten.");
                Console.Write("Default Channel: ");
            }

            // Pfad für die Konfigurationsdatei festlegen. ../Eigene Dokumente/IrcBot/<Dateiname>
            string saveDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + Path.DirectorySeparatorChar + "IrcBot" + Path.DirectorySeparatorChar;
            Console.Write("Konfiguration speichern unter: " + saveDirectory);
            fileName = Console.ReadLine();

            try 
            {
                // Prüfen, ob das Verzeichnis bereits existiert. Ansonsten neu erstellen.
                if (!Directory.Exists(saveDirectory))
                    Directory.CreateDirectory(saveDirectory);

                // Configurationsdatei mit Hilfe unserer statischen Funktion speichern
                ClientConfiguration.SaveConfig(saveDirectory + fileName, new Irc(server, port, nickname, realname, defaultChannel));
                Console.WriteLine("Konfigurationsdatei wurde angelegt. Bitte starten Sie das Programm zukuenftig mit dem Parameter cfg=" + saveDirectory + fileName);
                Console.ReadKey();

                // Die globale Variable festlegen und so tun als ob der Benutzer den cfg= Parameter benutzt hätte.
                ConfigFile = saveDirectory + fileName;
            }
            catch (Exception ex)
            {
                // Im Fehlerfall die Fehlermeldung anzeigen und das Programm beenden.
                Console.WriteLine("Fehler beim Speichern der Konfigurationsdatei!");
                Console.WriteLine(ex.Message);
                Console.ReadKey();
                Environment.Exit(1);
            }

        }

        private void GetData()
        {
            #region Lade Konfiguration wenn vorhanden. Ansonsten nutze Standartwerte
            if (ConfigFile != null)
                IrcConnection = Irc.LoadConfig(ConfigFile);

            #endregion
            Console.ReadKey();
            // Verbindung zum IRC-Server aufbauen
            IrcConnection.Connect();

            // Verbindung prüfen
            if (!IrcConnection.Connected)
            {
                Console.WriteLine("VERBINDUNG ZUM SERVER KONNTE NICHT HERGESTELLT WERDEN.");
                Console.ReadKey();
                return;
            }

            while(true)
            {
                string Data = IrcConnection.readData();

                if(Data != null)
                {
                    Console.WriteLine("> " + Data);

 
                    #region PING
                    if (Data.Substring(0,6) == "PING :")
                    {
                        string[] tPing = Data.Split(Convert.ToChar(":"));
                        IrcConnection.sendData("PONG :" + tPing[1]);
                    }
                    #endregion
                    #region Server Messages
                    if (Regex.IsMatch(Data, @":(.+) ([\d]{3}) (.+) :(.+)"))
                    {
                        string[] tRegEx = Regex.Split(Data, @":(.+) ([\d]{3}) (.+) :(.+)");
                        // tRegEx[0] = null
                        // tRegEx[1] = Server-Hostname
                        // tRegEx[2] = Numeric Response
                        // tRegEx[3] = Client Username
                        // tRegEx[4] = Server Message
                        // tRegEx[5] = null


                        if (tRegEx[2] == "001")
                        {
                            IrcConnection.ServerAdress = tRegEx[1];
                        }
                        else if(tRegEx[2] == "376")
                        {
                            IrcConnection.joinRoom("#PatricksTestRoom");
                            IrcConnection.ChatMessage("Dies ist eine Testnachricht", "#PatricksTestRoom");
                        }
                        
                    }
                    #endregion

                    #region Vorbereitung Folge #003 - IrcMessages
                    if (Regex.IsMatch(Data, @":([^@!\ ]*)(.)*PRIVMSG\ ([^@!\ ]*)\ :"))
                    {
                        // :|54H|DamianRyse!~|54H|Dami@DamianRyse.users.quakenet.org PRIVMSG #DamiansTestRoom :test
                        IrcMessage Message = new IrcMessage(Data);
                        Actions(Message);
                        
                    }
                    #endregion


                }
            }
        }

        private void Actions(IrcMessage Message)
        {
            // RegularExpression-Patterns für das Prüfen von Benutzer- und Channelnamen.
            const string RegEx_Username = "(.[^,# ]+)"; // Alle Zeichen ausser , # und Leerzeichen.
            const string RegEx_Channelname = "#(.[^, ]+)"; // Ein # muss vorangestellt sein, es darf kein , und kein Leerzeichen im Namen sein.

            // Prüfen, ob Nachricht einen gültigen Befehl enthält und dann entsprechend darauf reagieren.
            #region !hello
            if (Message.Message.ToLower() == "!hello")
            {
                IrcConnection.ChatMessage("Hallo " + Message.Author + ", ich bin ein IRC Bot. Ich kann mich zu einem beliebigen IRC-Server verbinden und reagiere bereits auf einige Befehle.", Message.Channel);
                IrcConnection.ChatMessage("Ausserdem bin ich Teil eines YouTube-Tutorials!", Message.Channel);
            }
            #endregion

            #region !quit || !exit
            if (Message.Message.ToLower() == "!quit" || Message.Message.ToLower() == "!exit")
            {
                // An dieser Stelle ist es sinnvoll, eine Überprüfung einzubauen, ob der User überhaupt berechtigt ist,
                // diesen Befehl auszuführen. Dies könnte man zum Beispiel über ein integriertes Benutzer-System
                // realisieren. User müssen sich dann bei bestimmten Befehlen vorher beim Bot "einloggen" um ihn verwalten
                // zu können. Eine andere Möglichkeit wäre, das nur Operatoren (Benutzer mit dem Flag +o) diese Befehle
                // ausführen dürfen.

                // Verabschiedung im IRC-Kanal
                IrcConnection.ChatMessage("Programm beendet. Auf wiedersehen!", Message.Channel);

                // Dem Server mitteilen, das die Verbindung getrennt werden soll.
                IrcConnection.sendData("QUIT");

                // Das Programm beenden.
                Environment.Exit(0);
            }
            #endregion

            #region !join #room
            if (Regex.IsMatch(Message.Message, @"^!(?i)join " + RegEx_Channelname + "$"))
            {
                // An dieser Stelle ist es sinnvoll, eine Überprüfung einzubauen, ob der User überhaupt berechtigt ist,
                // diesen Befehl auszuführen. Dies könnte man zum Beispiel über ein integriertes Benutzer-System
                // realisieren. User müssen sich dann bei bestimmten Befehlen vorher beim Bot "einloggen" um ihn verwalten
                // zu können. Eine andere Möglichkeit wäre, das nur Operatoren (Benutzer mit dem Flag +o) diese Befehle
                // ausführen dürfen.

                string[] args = Regex.Split(Message.Message, @"^!(?i)join " + RegEx_Channelname + "$");

                IrcConnection.ChatMessage("Joining channel #" + args[1] + "...", Message.Channel);
                IrcConnection.joinRoom("#" + args[1]);
                return;
            }
            #endregion

            #region !leave
            if (Message.Message.ToLower() == "!leave")
            {
                // An dieser Stelle ist es sinnvoll, eine Überprüfung einzubauen, ob der User überhaupt berechtigt ist,
                // diesen Befehl auszuführen. Dies könnte man zum Beispiel über ein integriertes Benutzer-System
                // realisieren. User müssen sich dann bei bestimmten Befehlen vorher beim Bot "einloggen" um ihn verwalten
                // zu können. Eine andere Möglichkeit wäre, das nur Operatoren (Benutzer mit dem Flag +o) diese Befehle
                // ausführen dürfen.

                IrcConnection.ChatMessage("Leaving channel. Bye!", Message.Channel);
                IrcConnection.sendData("PART " + Message.Channel);
                return;
            }
            #endregion
        }
    }
}
