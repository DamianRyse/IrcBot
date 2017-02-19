// ClientConfiguration.cs
// Enthält die Klasse ClientConfiguration mit einigen untergeordnen Klassen
// welche Informationen zum IRC-Server enhält, mit dem sich der Bot beim
// Programmstart  verbinden soll. Darüber hinaus werden hier auch erweiterte
// Einstellungen gespeichert, wie alle registrierten Superuser, alle Channels,
// die beim Start automatisch betreten werden sollen und alle Zitate, die 
// im Laufe der Anwendung eingetragen wurden und werden.

// Dadurch, das alle relevanten Informationen in Subklassen gespeichert sind
// und mit der statischen Methode SaveConfig die Hauptklasse serialisiert
// wird, lässt uns das die Möglichkeit der dynamischen Erweiterbarkeit der 
// Klasse ClientConfiguration offen.

using System.IO;
using System.Web.Script.Serialization;

namespace IrcBot
{
    class ClientConfiguration
    {
        public serverDetails ServerDetails { get; set; }
        public quote[] Quotes { get; set; }
        public superUser[] SuperUsers { get; set; }
        public channel[] Channels { get; set; }
        public customAction[] CustomActions { get; set; }

        public class serverDetails
        {
            private const string _passphrase = "nu0ME8sKD53AWW8XGlp2SUlwo#7h!3";
            /// <summary>
            /// Beschreibt die Adresse zum IRC-Server
            /// </summary>
            public string ServerURL { get; set; }
            /// <summary>
            /// Beschreibt den Port zum IRC-Server. Standart: 6667
            /// </summary>
            public int ServerPort { get; set; }
            /// <summary>
            /// Beschreibt den Nickname, den der Bot beim Anmelden am Server nutzen soll.
            /// </summary>
            public string Nick { get; set; }
            /// <summary>
            /// Beschreibt den vollen Benutzernamen, den der Bot beim Anmelden am Server nutzen soll.
            /// </summary>
            public string User { get; set; }
            /// <summary>
            /// Beschreibt den Nickname, der zum Login bei Q genutzt werden soll.
            /// </summary>
            public string AuthNick { get; set; }
            /// <summary>
            /// Liest oder legt fest, welches beim Login zu Q genutzt werden soll. Das Passwort wird mit 256 Bit verschlüsselt.
            /// </summary>
            public string AuthPass
            {
                get
                {
                    if (!string.IsNullOrEmpty(_authPass))
                        return _authPass;
                    else
                        return null;
                }
                set
                {
                    if (!string.IsNullOrEmpty(value))
                        _authPass = StringCipher.Encrypt(value, _passphrase);
                    else
                        _authPass = null;
                }
            }
            private string _authPass;
            /// <summary>
            /// Liest und entschlüsselt das Passwort für die Authentifizierung bei Q.
            /// </summary>
            /// <returns></returns>
            public string GetAuthPass()
            {
                if (!string.IsNullOrEmpty(_authPass))
                    return StringCipher.Decrypt(_authPass, _passphrase);
                else
                    return null;
            }
            /// <summary>
            /// Liest oder legt fest, ob sich der Bot beim Start automatisch bei Q anmelden soll.
            /// </summary>
            public bool AuthAtStart { get; set; }
        }

        public class quote
        {
            public string Author { get; set; }
            public string Message { get; set; }
            public int Timestamp { get; set; }
        }

        public class superUser
        {
            private const string _passphrase = "nu0ME8sKD53AWW8XGlp2SUlwo#7h!3";
            private System.Timers.Timer logoutTimer = new System.Timers.Timer(1800000); // 30 Minuten

            public string Username { get; set; }
            public string Password { get; set; }
            [ScriptIgnore]
            public bool isLoggedIn { get; private set; }
            [ScriptIgnore]
            public string loggedInUser { get; private set; }

            public superUser()
            {
                logoutTimer.Elapsed += logoutTimer_Elapsed;
            }

            /// <summary>
            /// Loggt nacht 30 Minuten den User automatisch wieder aus.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="args"></param>
            private void logoutTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs args)
            {
                LogOut();
            }

            /// <summary>
            /// Gibt das entschlüsselte Passwort für diesen Superuser zurück.
            /// </summary>
            /// <returns></returns>
            public string GetPassword()
            {
                if (!string.IsNullOrEmpty(Password))
                    return StringCipher.Decrypt(Password, _passphrase);
                else
                    return null;
            }

            /// <summary>
            /// Setzt ein neues Passwort und verschlüsselt es.
            /// </summary>
            /// <param name="newPassword"></param>
            public void SetPassword(string newPassword)
            {
                if (!string.IsNullOrEmpty(newPassword))
                    Password = StringCipher.Encrypt(newPassword, _passphrase);
                else
                    Password = null;
            }

            /// <summary>
            /// Prüft das angegebene Passwort auf Korrektheit und loggt den User ein.
            /// </summary>
            /// <param name="Password"></param>
            /// <returns></returns>
            public bool LogIn(string Nickname, string Password)
            {
                // Passwort prüfen
                if (Password == GetPassword())
                {
                    // Nickname darf nicht eingeloggt sein oder muss der selbe Nickname sein, der bereits eingeloggt ist.
                    if (string.IsNullOrWhiteSpace(loggedInUser) || loggedInUser == Nickname)
                    {
                        isLoggedIn = true;
                        logoutTimer.Start();
                        loggedInUser = Nickname;
                        return true;
                    }
                    else
                        return false;
                }
                else
                {
                    return false;
                }
            }

            public void LogOut()
            {
                logoutTimer.Stop();
                isLoggedIn = false;
                loggedInUser = null;
            }
        }

        public class channel
        {
            private const string _passphrase = "nu0ME8sKD53AWW8XGlp2SUlwo#7h!3";

            public string Name { get; set; }
            private string _password;
            public string Password
            {
                get
                {
                    if (!string.IsNullOrEmpty(_password))
                        return _password;
                    else
                        return null;
                }
                set
                {
                    if (!string.IsNullOrEmpty(value))
                        _password = StringCipher.Encrypt(value, _passphrase);
                    else
                        _password = null;
                }
            }
            /// <summary>
            /// Gibt das entschlüsselte Passwort für diesen Kanal zurück.
            /// </summary>
            /// <returns></returns>
            public string GetPassword()
            {
                if (!string.IsNullOrEmpty(_password))
                    return StringCipher.Decrypt(_password, _passphrase);
                else
                    return null;
            }
            public bool Autojoin { get; set; }
        }

        public class customAction
        {
            public string Command { get; set; }
            public string Response { get; set; }
            public bool SuperuserOnly { get; set; }
            public string[] Placeholders { get; set; }
            
            public string GetResponse(params string[] Parameters)
            {
                // Sollte Response keinen Wert enthalten, null zurückgeben.
                if (string.IsNullOrEmpty(Response))
                    return null;

                string returnVal = Response;

                // Wenn mindestens 1 Parameter angegeben wurde...
                if (Parameters != null && Parameters.Length > 0)
                {
                    // -> Es existieren Platzhalter. Es dürfen nicht mehr Parameter als Platzhalter sein.
                    if (Placeholders != null && Placeholders.Length > 0 && Parameters.Length <= Placeholders.Length)
                    {
                        for (int i = 0; i < Parameters.Length; i++)
                        {
                            returnVal = returnVal.Replace(Placeholders[i], Parameters[i]);
                        }
                    }
                    else
                        returnVal = null;
                }
                return returnVal;
            }

        }

        public static ClientConfiguration LoadConfig(string file)
        {
            // Prüfen ob die Datei existiert
            if (File.Exists(file))
            {
                // JavaScriptSerializer initialisieren
                var jss = new JavaScriptSerializer();

                // Datei einlesen und als dynamic-Objekt zwischenspeichern
                return jss.Deserialize<ClientConfiguration>(File.ReadAllText(file));
            }
            else
                return null;
        }

        public static void SaveConfig(string fileName, ClientConfiguration Configuration)
        {
            var jss = new JavaScriptSerializer();
            File.WriteAllText(fileName, jss.Serialize(Configuration));

        }
    }
}
