using System.IO;
using System.Web.Script.Serialization;

namespace IrcBot
{
    class ClientConfiguration
    {
        public string IrcServer { get; set; }
        public string DefaultChannel { get; set; }
        public string Nickname { get; set; }
        public string Fullname { get; set; }
        public string Auth_Username { get; set; }
        public string Auth_Password { get; set; }
        public bool Auth_AtStart { get; set; }
        public int Port { get; set; }

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
