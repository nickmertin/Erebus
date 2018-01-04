using System;

namespace Vex472.Erebus.Client.Windows.DataModel
{
    public sealed class Message
    {
        public bool Received { get; set; }

        public string Text { get; set; }

        public Tuple<string, string, bool> AttachmentData { get; set; }

        public DateTime Timestamp { get; set; }
    }
}