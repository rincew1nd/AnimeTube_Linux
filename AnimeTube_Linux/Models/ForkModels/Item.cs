using Newtonsoft.Json;
using System.Collections.Generic;

namespace AnimeTube_Linux.Models.ForkModels
{
    public class Item
    {
        [JsonProperty(PropertyName = "playlist_name")]
        public string PlaylistName;
        [JsonProperty(PropertyName = "next_page_url")]
        public string NextPageUrl;
        [JsonProperty(PropertyName = "channels")]
        public List<Channel> Channels = new List<Channel>();
        [JsonProperty(PropertyName = "cacheinfo")]
        public string CacheInfo;
        [JsonProperty(PropertyName = "cachetime")]
        public int CacheTime;
        [JsonProperty(PropertyName = "cacheend")]
        public int CacheEnd;
    }
}