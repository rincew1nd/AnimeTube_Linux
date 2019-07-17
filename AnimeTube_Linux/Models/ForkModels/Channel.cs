using Newtonsoft.Json;

namespace AnimeTube_Linux.Models.ForkModels
{
    public class Channel
    {
        [JsonProperty(PropertyName = "title")]
        public string Title;
        [JsonProperty(PropertyName = "logo_30x30")]
        public string Logo;
        [JsonProperty(PropertyName = "description")]
        public string Description;
        [JsonProperty(PropertyName = "stream_url")]
        public string StreamUrl;
        [JsonProperty(PropertyName = "playlist_url")]
        public string PlaylistUrl;
        [JsonProperty(PropertyName = "search_on")]
        public string SearchOn;
    }
}