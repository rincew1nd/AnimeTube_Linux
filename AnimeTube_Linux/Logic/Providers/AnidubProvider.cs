using AnimeTube_Linux.Models;
using HtmlAgilityPack;
using NLog;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace AnimeTube_Linux.Logic.Providers
{
    [Provider("Anidub", "https://online.anidub.com", "https://online.anidub.com/favicon.ico")]
    public class AnidubProvider : IProvider
    {
        private static AnidubProvider _instance;
        private static object _lock = new object();
        public static AnidubProvider GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new AnidubProvider();
                    }
                }
            }
            return _instance;
        }
        private AnidubProvider() { }

        public ProviderAttribute Info() => typeof(AnidubProvider).GetCustomAttribute<ProviderAttribute>(false);
        
        public Series[] SearchForSeries(string name)
        {
            var series = new List<Series>();

            var doc = new HtmlDocument();
            var test = Info().SiteUrl;
            try
            {
                doc.LoadHtml(
                    HtmlUtils.GetHtmlPage(
                        Info().SiteUrl,
                        formData: new [] {
                            ("do","search"),
                            ("subaction","search"),
                            ("story", name),
                            ("x", "0"),
                            ("y", "0")
                        }
                ));
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex, "Ошибка при получении страницы");
            }

            var count = doc.DocumentNode.SelectNodes($"//div[contains(@class, 'newstitle')]")?.Count;
            
            for (var i = 1; i <= count; i++)
            {
                try
                {
                    var titleNode = doc.DocumentNode.SelectSingleNode($"(//div[contains(@class, 'newstitle')])[{i}]/div/div/a");
                    var title = titleNode.InnerText.Split('[')[0].TrimEnd(' ');
                    var link = titleNode.Attributes["href"].Value;

                    var imageNode = doc.DocumentNode.SelectSingleNode($"(//div[contains(@class, 'poster_img')])[{i}]/img");
                    var imageUrl = imageNode.Attributes["src"].Value;

                    var description = doc.DocumentNode.SelectSingleNode($"(//div[contains(@id, 'news-id-')])[{i}]").InnerText;

                    series.Add(new Series()
                    {
                        Name = title,
                        SeriesUrl = link,
                        ImageUrl = imageUrl,
                        Description = description
                    });
                }
                catch (Exception ex)
                {
                    LogManager.GetCurrentClassLogger().Error(ex, "Ошибка при парсинге сериала");
                }
            }

            return series.ToArray();
        }

        public Episode[] SearchForEpisode(string url)
        {
            var episodes = new List<Episode>();

            var doc = new HtmlDocument();
            doc.LoadHtml(HtmlUtils.GetHtmlPage(url));

            var selects = doc.DocumentNode.SelectNodes("//select[contains(@id, sel)]");

            if (selects != null)
            {
                foreach (var select in selects)
                {
                    var options = select.SelectNodes(".//option");
                    foreach (var option in options)
                    {
                        var link = Regex.Match(option.Attributes["value"].Value, "h?t?t?p?s?:?\\/?\\/?(.+?)\\s*\\|").Groups[1].Value;

                        if (!link.Contains("www.stormo.tv") && !link.Contains("video.sibnet.ru"))
                            continue;

                        link = option.Attributes["value"].Value.StartsWith("http:") ? $"http://{link}" : $"https://{link}";

                        episodes.Add(new Episode()
                        {
                            Name = option.InnerText,
                            MovieUrl = link,
                            IsDirectLink = false
                        });
                    }
                }
            }

            return episodes.ToArray();
        }
    }
}