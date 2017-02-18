// IrcMessage.cs
// Stellt eine Klasse für die PRIVMSG-Datenströme bereit

using System;

namespace IrcBot
{
    class IrcMessage
    {
        public string Author { get; }
        public string Channel { get; }
        public string Message { get; }
        public DateTime DatumZeit { get; }

        public IrcMessage(string Data)
        {
            string[] RegExSplit = System.Text.RegularExpressions.Regex.Split(Data, @":([^@!\ ]*)(.)*PRIVMSG\ ([^@!\ ]*)\ :");
            this.Author = RegExSplit[1];
            this.Channel = RegExSplit[3];
            this.Message = RegExSplit[4];
            DatumZeit = DateTime.Now;
        }
        public IrcMessage(string author, string channel, string message)
        {
            this.Author = author;
            this.Channel = channel;
            this.Message = message;
            this.DatumZeit = DateTime.Now;
        }
    }
}
