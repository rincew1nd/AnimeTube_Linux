using AnimeTube_Linux.Logic;
using AnimeTube_Linux.Models.ForkModels;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace AnimeTube_Linux.Controllers
{
    [Route("api/direct_link", Name = "DirectLink")]
    public class DirectLinkController : ControllerBase
    {
        [HttpGet, Route("")]
        public Item Index(string series, string episode, string url)
        {
            var item = new Item()
            {
                PlaylistName = $"{series} - {episode}"
            };

            var links = DirectLinkFinder.Find(url);
            if (links?.Any() ?? false)
            {
                foreach(var link in links)
                {
                    item.Channels.Add(new Channel()
                    {
                        Title = link.Item1,
                        StreamUrl = link.Item2
                    });
                }
            }
            else
            {
                item.Channels.Add(new Channel()
                {
                    Title = "Видео не было найдено"
                });
            }
            return item;
        }
    }
}