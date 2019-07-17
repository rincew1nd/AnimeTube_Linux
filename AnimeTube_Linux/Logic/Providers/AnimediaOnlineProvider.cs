using AnimeTube_Linux.Models;
using HtmlAgilityPack;
using NLog;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace AnimeTube_Linux.Logic.Providers
{
    [Provider("AnimediaOnline", "https://animedia.online/", "https://animedia.online/templates/Animedia/images/favicon.png")]
    public class AnimediaOnlineProvider : IProvider
    {
        private static AnimediaOnlineProvider _instance;
        private static object _lock = new object();
        public static AnimediaOnlineProvider GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new AnimediaOnlineProvider();
                    }
                }
            }
            return _instance;
        }

        public ProviderAttribute Info() => typeof(AnimediaOnlineProvider).GetCustomAttribute<ProviderAttribute>(false);

        public Series[] SearchForSeries(string name)
        {
            var series = new List<Series>();

            var doc = new HtmlDocument();
            try
            {
                doc.LoadHtml(
                    HtmlUtils.GetHtmlPage(Utils.Combine(Info().SiteUrl, $"/index.php?do=search"),
                    formData: new[] {
                        ("do","search"),
                        ("subaction","search"),
                        ("story", name),
                        ("x", "0"),
                        ("y", "0")
                    })
                );
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex, $"Ошибка при получении страницы {name}");
            }

            var nodes = doc.DocumentNode.SelectNodes("//a[contains(@class,'sres-wrap')]");
            if (nodes != null && nodes.Count > 0)
            {
                foreach (var node in nodes)
                {
                    try
                    {
                        var imageNode = node.SelectSingleNode("./div[@class='sres-img']/img");
                        var imageUrl = Utils.Combine(Info().SiteUrl, imageNode.Attributes["src"].Value);

                        var titleNode = node.SelectSingleNode("./div[@class='sres-text']/h2");
                        var title = titleNode.InnerText;

                        var link = node.Attributes["href"].Value;

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

            var nodes = doc.DocumentNode.SelectNodes("//div[@class='playlist-episodes']/a");
            if (nodes != null && nodes.Count > 0)
            {
                foreach (var node in nodes)
                {
                    try
                    {
                        var episodeName = node.InnerText;
                        var episodeUrl = node.Attributes["href"].Value;

                        episodes.Add(new Episode()
                        {
                            Name = episodeName,
                            MovieUrl = episodeUrl,
                            IsDirectLink = false
                        });

                    }
                    catch (Exception ex)
                    {
                        LogManager.GetCurrentClassLogger().Error(ex, "Ошибка парсинга эпизодов");
                    }
                }
            }

            return episodes.ToArray();
        }
    }
}