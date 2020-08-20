using System.ComponentModel.DataAnnotations;

namespace CyberButler.Entities
{
    internal class CustomCommand
    {
        public string Server { get; set; }

        public string Command { get; set; }
        public string Text { get; set; }
    }
}