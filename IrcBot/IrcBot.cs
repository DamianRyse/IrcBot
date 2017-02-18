//////////////////////////////////////
//                                  // 
//        IRC-Bot Tutorial          //
//        für C# / .NET             //
//                                  //
//////////////////////////////////////


using System;
using System.Text.RegularExpressions;
using System.IO;

namespace IrcBot
{
    class IrcBot
    {
        Irc IrcConnection;
        ClientConfiguration clientConfig = null;
        string ConfigFile { get; set; }


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

            string fileName;
            int port; // als Zwischenspeicher benötigt, da eine Eigenschaft nicht als out-Parameter genutzt werden kann.
            ClientConfiguration newConfig = new ClientConfiguration();
            newConfig.ServerDetails = new ClientConfiguration.serverDetails();

            newConfig.Channels = new ClientConfiguration.channel[1];
            newConfig.Channels[0] = new ClientConfiguration.channel();

            newConfig.SuperUsers = new ClientConfiguration.superUser[1];
            newConfig.SuperUsers[0] = new ClientConfiguration.superUser();

            newConfig.Channels = new ClientConfiguration.channel[1];
            newConfig.Channels[0] = new ClientConfiguration.channel();

            newConfig.Quotes = new ClientConfiguration.quote[0];




            Console.Write("IRC-Server Hostname: ");
            newConfig.ServerDetails.ServerURL = Console.ReadLine();

            Console.Write("IRC-Server Port: ");
            while(!int.TryParse(Console.ReadLine(),out port) || port < 0 || port > 65535)
            {
                Console.WriteLine("Ungültige Portangabe. Bitte geben Sie einen Port von 0 bis 65535 ein.");
                Console.Write("IRC-Server Port: ");
            }
            newConfig.ServerDetails.ServerPort = port;

            Console.Write("Bot Nickname: ");
            while(!Regex.IsMatch(newConfig.ServerDetails.Nick = Console.ReadLine(), @"^(.[^ ,#]+)$"))
            {
                Console.WriteLine("Ungüliges Format für den Nickname. Ein Nickname darf keine Kommata, kein #-Symbol und kein Leerzeichen enthalten.");
                Console.Write("Bot Nickname: ");
            }
            

            Console.Write("Bot Realname: ");
            newConfig.ServerDetails.User = Console.ReadLine();

            Console.Write("Default Channel: ");
            while (!Regex.IsMatch(newConfig.Channels[0].Name = Console.ReadLine(), "^#(.[^, ]+)$"))
            {
                Console.WriteLine("Ungültiger Channelname. Ein Channel beginnt mit einem #-Symbol, darf keine Kommata und keine Leerzeichen enthalten.");
                Console.Write("Default Channel: ");
            }

            Console.Write("Default Channel Password (leave empty for none): ");
            newConfig.Channels[0].Password = Console.ReadLine();
            newConfig.Channels[0].Autojoin = true;

            Console.Write("Default Superuser: ");
            while (!Regex.IsMatch(newConfig.SuperUsers[0].Username = Console.ReadLine(), "^([a-zA-Z0-9]+)$"))
            {
                Console.WriteLine("Ungültiger Superuser-Name. Erlaubte Zeichen: a-z, A-Z und 0-9");
                Console.Write("Default Channel: ");
            }
            Console.Write("Default Superuser Password: ");
            newConfig.SuperUsers[0].SetPassword(Console.ReadLine());

            // Pfad für die Konfigurationsdatei festlegen. ../Eigene Dokumente/IrcBot/<Dateiname>
            string saveDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + Path.DirectorySeparatorChar + "IrcBot" + Path.DirectorySeparatorChar;
            Console.Write("Konfiguration speichern unter: " + saveDirectory);
            fileName = Console.ReadLine();

            try 
            {
                // Prüfen, ob das Verzeichnis bereits existiert. Ansonsten neu erstellen.
                if (!Directory.Exists(saveDirectory))
                    Directory.CreateDirectory(saveDirectory);

                // Konfigurationsdatei mit Hilfe unserer statischen Funktion speichern
                ClientConfiguration.SaveConfig(saveDirectory + fileName,newConfig);
                Console.WriteLine("Konfigurationsdatei wurde angelegt. Bitte starten Sie das Programm zukuenftig mit dem Parameter cfg=" + saveDirectory + fileName);
                Console.ReadKey();

                // Programm beenden.
                Environment.Exit(1);
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
            if (!string.IsNullOrWhiteSpace(ConfigFile))
            {
                try
                {
                    clientConfig = ClientConfiguration.LoadConfig(ConfigFile);
                    IrcConnection = new Irc(clientConfig);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Fehler beim Laden der Konfiguration: " + ex.Message);
                    Console.ReadKey();
                    return; // return beendet die Methode und damit das Programm.
                }
            }
               

            #endregion

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

                if(!string.IsNullOrWhiteSpace(Data))
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
                            AutoJoinChannels(clientConfig.Channels);
                        }
                    }
                    #endregion

                    #region PRIVMSG
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
                if(IsUserLoggedIn(Message.Author,clientConfig.SuperUsers) != null)
                {
                    // Verabschiedung im IRC-Kanal
                    IrcConnection.ChatMessage("Programm beendet. Auf wiedersehen!", Message.Channel);

                    // Dem Server mitteilen, das die Verbindung getrennt werden soll.
                    IrcConnection.sendData("QUIT");

                    // Das Programm beenden.
                    Environment.Exit(0);
                }
                else
                {
                    IrcConnection.ChatMessage("Nur angemeldete Superuser dürfen diesen Befehl ausführen.", Message.Channel);
                    return;
                }
            }
            #endregion

            #region !join #room
            if (Regex.IsMatch(Message.Message, @"^!(?i)join " + RegEx_Channelname + "$"))
            {
                if(IsUserLoggedIn(Message.Author,clientConfig.SuperUsers) != null)
                {
                    string[] args = Regex.Split(Message.Message, @"^!(?i)join " + RegEx_Channelname + "$");

                    IrcConnection.ChatMessage("Joining channel #" + args[1] + "...", Message.Channel);
                    IrcConnection.joinRoom("#" + args[1]);
                    return;
                }
                else
                {
                    IrcConnection.ChatMessage("Nur angemeldete Superuser dürfen diesen Befehl ausführen.", Message.Channel);
                    return;
                }
            }
            #endregion

            #region !leave
            if (Message.Message.ToLower() == "!leave")
            {
                if(IsUserLoggedIn(Message.Author,clientConfig.SuperUsers) != null)
                {
                    IrcConnection.ChatMessage("Verlasse den Channel. Bye!", Message.Channel);
                    IrcConnection.sendData("PART " + Message.Channel);
                    return;
                }
                else
                {
                    IrcConnection.ChatMessage("Nur angemeldete Superuser dürfen diesen Befehl ausführen.", Message.Channel);
                    return;
                }
            }
            #endregion

            #region !login <UserName> <Password>
            if (Regex.IsMatch(Message.Message, @"!(?i)login ([a-zA-Z0-9]+) (.)"))
            {
                string[] args = Regex.Split(Message.Message, @"!(?i)login ([a-zA-Z0-9]+) (.+)");
                Console.WriteLine("Benutzername: " + args[1]);
                Console.WriteLine("Passwort: " + args[2]);
                foreach (ClientConfiguration.superUser user in clientConfig.SuperUsers)
                {
                    if (!string.IsNullOrWhiteSpace(user.Username) && user.Username == args[1])
                    {
                        if (user.LogIn(Message.Author, args[2]))
                        {
                            IrcConnection.ChatMessage("Login erfolgreich. Du wirst automatisch nach 30 Minuten ausgeloggt.", Message.Author);
                            IrcConnection.ChatMessage("Oder Benutze !logout, wenn Du Dich vorher abmelden möchtest.", Message.Author);
                            return;
                        }
                    }
                }
                IrcConnection.ChatMessage("Benutzername und/oder Passwort falsch.", Message.Author);
                return;
            }
            #endregion

            #region !logout
            if (Message.Message.ToLower() == "!logout")
            {
                ClientConfiguration.superUser user;
                if ((user = IsUserLoggedIn(Message.Author,clientConfig.SuperUsers)) != null)
                {
                    user.LogOut();
                    IrcConnection.ChatMessage("Du wurdest erfolgreich abgemeldet.", Message.Author);
                    return;
                }
                else
                {
                    IrcConnection.ChatMessage("Du bist nicht eingeloggt.", Message.Author);
                    return;
                }
            }
            #endregion

        }

        private void AutoJoinChannels(ClientConfiguration.channel[] channels)
        {
            // Prüfen ob ein Channel-Array existiert und Werte beinhaltet
            if(channels != null && channels.Length > 0)
            {
                // Alle Channels durchlaufen
                foreach(ClientConfiguration.channel channel in channels)
                {
                    // Wenn Channel-Eigenschaft Autojoin = true, dann Channel betreten.
                    if(channel.Autojoin)
                    {
                        // Prüfen, ob ein Passwort gesetzt wurde.
                        if (string.IsNullOrWhiteSpace(channel.Password))
                            IrcConnection.joinRoom(channel.Name + channel.Password);
                        else
                            IrcConnection.joinRoom(channel.Name + " " + channel.GetPassword());
                    }
                }
            }
        }

        /// <summary>
        /// Prüft, ob der Benutzer eingeloggt ist und gibt das entsprechende superUser-Objekt zurück. Andernfalls wird null zurückgegeben.
        /// </summary>
        /// <param name="Nickname">Der Nickname, der überprüft werden soll.</param>
        /// <param name="Superusers">Das Array mit den gespeicherten Superusern.</param>
        /// <returns></returns>
        private ClientConfiguration.superUser IsUserLoggedIn(string Nickname, ClientConfiguration.superUser[] Superusers)
        {
            if (Superusers != null && Superusers.Length > 0)
            {
                foreach(ClientConfiguration.superUser user in Superusers)
                {
                    if (user.isLoggedIn && user.loggedInUser == Nickname)
                        return user;
                }
                return null;
            }
            else
                return null;
        }
    }
}
