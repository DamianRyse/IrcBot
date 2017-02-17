using System.IO;
using System.Web.Script.Serialization;

namespace IrcBot
{
    class ClientConfiguration
    {
        /// <summary>
        /// Beschreibt die Adresse zum IRC-Server
        /// </summary>
        public string ServerURL { get; set; }
        /// <summary>
        /// Beschreibt den Port zum IRC-Server. Standart: 6667
        /// </summary>
        public int ServerPort { get; set; }
        /// <summary>
        /// Beschreibt den IRC-Channel, den der Bot automatisch betreten soll.
        /// </summary>
        public string DefaultChannel { get; set; }
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
                return StringCipher.Decrypt(_authPass, "nu0ME8sKD53AWW8XGlp2SUlwo#7h!3");
            }
            set
            {
                _authPass = StringCipher.Encrypt(value, "nu0ME8sKD53AWW8XGlp2SUlwo#7h!3");
            }
        }
        private string _authPass;
        /// <summary>
        /// Liest oder legt fest, ob sich der Bot beim Start automatisch bei Q anmelden soll.
        /// </summary>
        public bool AuthAtStart { get; set; }
        

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

        }
    }
}
