using Newtonsoft.Json;

namespace CyberButler
{
    // this class will hold data from config.json
    public class ConfigJson
    {
        [JsonProperty("DiscordToken")]
        public string DiscordToken { get; set; }

        [JsonProperty("CommandPrefix")]
        public string CommandPrefix { get; set; }

        [JsonProperty("SpotifyClientID")]
        public string SpotifyClientID { get; set; }

        [JsonProperty("SpotifyPlaylistID")]
        public string SpotifyPlaylistID { get; set; }
    }
}
