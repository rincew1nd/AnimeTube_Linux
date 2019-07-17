using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AnimeTube_Linux.Models;
using HtmlAgilityPack;
using Newtonsoft.Json;
using NLog;

namespace AnimeTube_Linux.Logic.Providers
{
    [Provider("AnimediaTV", "https://online.animedia.tv", "https://online.animedia.tv/favicon.ico")]
    public class AnimediaTvProvider : IProvider
    {
        private static AnimediaTvProvider _instance;
        private static object _lock = new object();
        public static AnimediaTvProvider GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new AnimediaTvProvider();
                    }
                }
            }
            return _instance;
        }

        public ProviderAttribute Info() => typeof(AnimediaTvProvider).GetCustomAttribute<ProviderAttribute>(false);
        
        public Series[] SearchForSeries(string name)
        {
            var series = new List<Series>();

            var json = HtmlUtils.GetHtmlPage(Utils.Combine(Info().SiteUrl, $"/ajax/url_s/search&keywords={name}"));
            dynamic jsonObj = JsonConvert.DeserializeObject(json);
            foreach (var item in jsonObj.items)
            {
                if (item.language == "Аниме онлайн")
                {
                    string title = item.name;
                    string link = item.html_url;
                    string imageUrl = item.image;

                    if (link.StartsWith("//"))
                        link = "http:" + link;

                    series.Add(new Series()
                    {
                        Name = title,
                        SeriesUrl = link,
                        ImageUrl = imageUrl
                    });
                }
            }

            #region Temporary broken full search
            /*
            var doc = new HtmlDocument();
            try
            {
                doc.LoadHtml(HtmlUtils.GetHtmlPage(Utils.Combine(Info().SiteUrl, $"/ajax/search_result_search_page/P0?keywords={name}")));
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex, $"Ошибка при получении страницы {name}");
            }

            var nodes = doc.DocumentNode.SelectNodes("//div[@class='ads-list__item']");
            if (nodes != null && nodes.Count > 0)
            {
                foreach(var node in nodes)
                {
                    try
                    {
                        var titleNode = node.SelectSingleNode("./a[contains(@class,'ads-list__item__title')]");
                        var title = titleNode.InnerText;
                        var link = titleNode.Attributes["href"].Value;

                        var imageNode = node.SelectSingleNode("./div/a/img");
                        var imageUrl = imageNode.Attributes["data-src"].Value;

                        series.Add(new Series()
                        {
                            Name = title,
                            SeriesUrl = link,
                            ImageUrl = imageUrl
                        });

                    }
                    catch (Exception ex)
                    {
                        LogManager.GetCurrentClassLogger().Error(ex, "Ошибка парсинга сериала");
                    }
                }
            }
            */
            #endregion

            return series.ToArray();
        }

        public Episode[] SearchForEpisode(string url)
        {
            var episodes = new List<Episode>();

            var doc = new HtmlDocument();
            try
            {
                doc.LoadHtml(HtmlUtils.GetHtmlPage(url));
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex, $"Ошибка при получении страницы {url}");
            }

            int seriesId = 0, playlistId = 0;

            // Episode Id - //ul[@role='tablist'] - @data-entry_id
            var seriesIdNode = doc.DocumentNode.SelectSingleNode("//ul[@role='tablist']");
            if (seriesIdNode == null)
                return episodes.ToArray();
            else
                seriesId = int.Parse(seriesIdNode.Attributes["data-entry_id"].Value);

            // Playlists Ids - //a[contains(@href,'#tab')] - @href (only numbers)
            var playlistNodes = doc.DocumentNode.SelectNodes("//a[contains(@href,'#tab')]");
            if (playlistNodes != null && playlistNodes.Count > 0)
            {
                foreach (var playlistNode in playlistNodes)
                {
                    try
                    {
                        playlistId = int.Parse(playlistNode.Attributes["href"].Value.Replace("#tab", "")) + 1;

                        // Episode file (JSON) - https://online.animedia.tv/embeds/playlist-j.txt/{seriesId}/{playlistId}
                        var json = HtmlUtils.GetHtmlPage(Utils.Combine(Info().SiteUrl, $"embeds/playlist-j.txt/{seriesId}/{playlistId}"));
                        dynamic jsonObj = JsonConvert.DeserializeObject($"{{\"episodes\":{json}}}");
                        foreach(var episode in jsonObj.episodes)
                        {
                            // Episode description - title
                            string title = episode.title;

                            // Link to m3u8 file - file
                            string file = episode.file;

                            if (file.StartsWith("//"))
                                file = "http:" + file;

                            episodes.Add(new Episode()
                            {
                                Name = title,
                                MovieUrl = file,
                                IsDirectLink = true
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        LogManager.GetCurrentClassLogger().Error(ex, $"Ошибка при обработке эпизодов {seriesId} {playlistId}");
                    }
                }
            }

            // Get all episodes - https://online.animedia.tv/ajax/episodes/{episodeId}/{playlistId}
            // Episode node - //div[@class='media__tabs__series__list__item']
            // Link to episode - ./a - @href (+provider url)
            // Preview image - ./a/img - @data-src
            // Episode description - ./a/img - @alt

            return episodes.ToArray();
        }
    }
}