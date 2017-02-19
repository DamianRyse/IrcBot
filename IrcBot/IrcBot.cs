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

            #region CustomActions
            if(Regex.IsMatch(Message.Message, "^!([a-zA-Z0-9]+)(.+)?$"))
            {
                string[] args = Regex.Split(Message.Message, @"^!([a-zA-Z0-9]+)(.+)?$");

                foreach (ClientConfiguration.customAction action in clientConfig.CustomActions)
                {
                    if(action.Command.ToLower() == "!" + args[1].ToLower())
                    {
                        if(action.SuperuserOnly && IsUserLoggedIn(Message.Author, clientConfig.SuperUsers) == null)
                        {
                            IrcConnection.ChatMessage("Nur angemeldete Superuser dürfen diesen Befehl ausführen.", Message.Channel);
                            break;
                        }

                        string response;
                        if (string.IsNullOrWhiteSpace(args[2]))
                            response = action.GetResponse();
                        else
                            response = action.GetResponse(args[2].Split(new char[] {' '},StringSplitOptions.RemoveEmptyEntries));

                        if(!string.IsNullOrWhiteSpace(response))
                            IrcConnection.ChatMessage(response, Message.Channel);

                    }
                }
            }
            #endregion

            #region !hello
            if (Message.Message.ToLower() == "!hello")
            {
                IrcConnection.ChatMessage("Hallo " + Message.Author + ", ich bin ein IRC Bot. Ich kann mich zu einem beliebigen IRC-Server verbinden und reagiere bereits auf einige Befehle.", Message.Channel);
                IrcConnection.ChatMessage("Ausserdem bin ich Teil eines YouTube-Tutorials!", Message.Channel);
            }
            #endregion

            #region !help
            if (Message.Message.ToLower() == "!help")
            {
                if(clientConfig.CustomActions != null && clientConfig.CustomActions.Length > 0)
                {
                    foreach(ClientConfiguration.customAction action in clientConfig.CustomActions)
                    {
                        string placeholder = "";
                        if(action.Placeholders != null && action.Placeholders.Length > 0)
                        {
                            for(int i = 0; i < action.Placeholders.Length; i++ )
                            {
                                placeholder += action.Placeholders[i] + " ";
                            }
                        }
                        // --> !Befehl %Params% -> Response Message
                        IrcConnection.ChatMessage("--> " + action.Command + " " + placeholder + "-> " + action.Response, Message.Channel);
                    }
                }
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

            #region !autojoin
            if (Message.Message.ToLower() == "!autojoin")
            {
                if (IsUserLoggedIn(Message.Author, clientConfig.SuperUsers) != null) // User muss eingeloggt sein
                {
                    if(clientConfig.Channels != null && clientConfig.Channels.Length > 0) // Prüfen ob Array null ist oder kein Element besitzt
                    {
                        foreach(ClientConfiguration.channel channel in clientConfig.Channels) // Schleife durch alle Channels
                        {
                            if (channel.Name.ToLower() == Message.Channel.ToLower()) // Channel wurde in der Konfiguration gefunden
                            {
                                if(channel.Autojoin)
                                {
                                    channel.Autojoin = false;
                                    IrcConnection.ChatMessage("Dieser Channel wird absofort nicht mehr automatisch betreten.", Message.Channel);
                                }
                                else
                                {
                                    channel.Autojoin = true;
                                    IrcConnection.ChatMessage("Dieser Channel wird absofort automatisch betreten",Message.Channel);
                                }
                                ClientConfiguration.SaveConfig(ConfigFile, clientConfig);
                                return;
                            }
                        }
                        // Channel wurde nicht im Array gefunden
                        clientConfig.Channels = toolbox.ResizeArray<ClientConfiguration.channel>(clientConfig.Channels, 1);
                        clientConfig.Channels[clientConfig.Channels.Length - 1] = new ClientConfiguration.channel();
                        clientConfig.Channels[clientConfig.Channels.Length - 1].Autojoin = true;
                        clientConfig.Channels[clientConfig.Channels.Length - 1].Name = Message.Channel;
                        ClientConfiguration.SaveConfig(ConfigFile, clientConfig);
                        IrcConnection.ChatMessage("Dieser Channel wird absofort automatisch betreten", Message.Channel);
                        return;
                    }
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

            #region !addAction <!command> <Response> -> <Params>
            if (Regex.IsMatch(Message.Message, @"^!(?i)addAction !([a-zA-Z0-9]+) (.*?)( \-> (.+))?$"))
            {
                if (IsUserLoggedIn(Message.Author, clientConfig.SuperUsers) != null)
                {
                    string[] args = Regex.Split(Message.Message, @"^!(?i)addAction !([a-zA-Z0-9]+) (.*?)( \-> (.+))?$");

                    ClientConfiguration.customAction newCustomAction;
                    if (clientConfig.CustomActions == null || clientConfig.CustomActions.Length < 1)
                    {
                        clientConfig.CustomActions = new ClientConfiguration.customAction[1];
                        newCustomAction = (clientConfig.CustomActions[0] = new ClientConfiguration.customAction());
                    }
                    else
                    {
                        clientConfig.CustomActions = toolbox.ResizeArray<ClientConfiguration.customAction>(clientConfig.CustomActions, 1);
                        clientConfig.CustomActions[clientConfig.CustomActions.Length - 1] = new ClientConfiguration.customAction();
                    }

                    // Keine Parameter
                    if (args.Length == 4)
                    {
                        clientConfig.CustomActions[clientConfig.CustomActions.Length - 1].Command = "!" + args[1];
                        clientConfig.CustomActions[clientConfig.CustomActions.Length - 1].Placeholders = null;
                        clientConfig.CustomActions[clientConfig.CustomActions.Length - 1].Response = args[2];
                    }
                    // Mit Parameter
                    else if (args.Length == 6)
                    {
                        clientConfig.CustomActions[clientConfig.CustomActions.Length - 1].Command = "!" + args[1];
                        clientConfig.CustomActions[clientConfig.CustomActions.Length - 1].Placeholders = args[4].Split(Convert.ToChar(" "));
                        clientConfig.CustomActions[clientConfig.CustomActions.Length - 1].Response = args[2];
                    }
                    // Konfigurationsdatei speichern
                    ClientConfiguration.SaveConfig(ConfigFile, clientConfig);
                    IrcConnection.ChatMessage("Erfolg: Action gespeichert!", Message.Channel);
                }
                else
                {
                    IrcConnection.ChatMessage("Nur angemeldete Superuser dürfen diesen Befehl ausführen.", Message.Channel);
                    return;
                }

            }
            else if(Regex.IsMatch(Message.Message, @"^!(?i)addAction (.+)?$"))
            {
                if (IsUserLoggedIn(Message.Author, clientConfig.SuperUsers) != null)
                {
                    IrcConnection.ChatMessage("Du hast den !addAction-Befehl falsch benutzt. Richtig sind folgende Schreibweisen:", Message.Channel);
                    IrcConnection.ChatMessage("-> !addAction !radio 5house.fm is ein geiler Radiosender", Message.Channel);
                    IrcConnection.ChatMessage("-> !addAction !radio %radio% ist ein geiler Radiosender -> %radio%", Message.Channel);
                    IrcConnection.ChatMessage("...wobei der Parameter-Name beliebig benannt werden kann. Es können auch mehrere Parameter verwendet werden.", Message.Channel);
                    IrcConnection.ChatMessage("-> !addaction !radio %radio% ist das geilste Radio der %Ort% -> %radio% %Ort% ", Message.Channel);
                }
                else
                {
                    IrcConnection.ChatMessage("Nur angemeldete Superuser dürfen diesen Befehl ausführen.", Message.Channel);
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
