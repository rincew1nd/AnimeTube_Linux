using AnimeTube_Linux.Models;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace AnimeTube_Linux.Logic.Providers
{
    [Provider("SovetRomantica", "https://sovetromantica.com", "https://sovetromantica.com//favicon.ico")]
    public class SovetRomanticaProvider : IProvider
    {
        private static SovetRomanticaProvider _instance;
        private static object _lock = new object();
        public static SovetRomanticaProvider GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new SovetRomanticaProvider();
                    }
                }
            }
            return _instance;
        }
        private SovetRomanticaProvider() { }

        public ProviderAttribute Info() => typeof(SovetRomanticaProvider).GetCustomAttribute<ProviderAttribute>(false);
        
        public Series[] SearchForSeries(string name)
        {
            var series = new List<Series>();

            var doc = new HtmlDocument();
            try
            {
                doc.LoadHtml(HtmlUtils.GetHtmlPage($"https://sovetromantica.com/anime?query={name}"));
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex, "Ошибка при получении страницы");
            }

            var nodes = doc.DocumentNode.SelectNodes($"//div[@class='anime--block__desu']");
            if (nodes == null || nodes.Count == 0)
                return series.ToArray();

            foreach(var node in nodes)
            {
                try
                {
                    var linkNode = node.SelectSingleNode(".//div/a");
                    var link = linkNode.Attributes["href"].Value;
                    
                    var imageUrl = Regex.Match(node.InnerHtml, "content=\"(https.+?)\"").Groups[1].Value;

                    var titleNode = node.SelectSingleNode(".//div/div[@class='anime--block__name']/span[2]");
                    var title = titleNode.InnerText;

                    series.Add(new Series()
                    {
                        Name = title,
                        SeriesUrl = link,
                        ImageUrl = imageUrl
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
            try
            {
                doc.LoadHtml(HtmlUtils.GetHtmlPage(Utils.Combine(Info().SiteUrl, url)));
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex, "Ошибка при получении страницы");
            }
            
            var nodes = doc.DocumentNode.SelectNodes($"//div[contains(@class,'episodes-slick_item')]");
            if (nodes != null && nodes.Count > 0)
                GetMultipleSeries(nodes, ref episodes);
            else
            {
                var node = doc.DocumentNode.SelectSingleNode("//div[@class='episode_info']/a[not(contains(@href,'#'))]");
                if (node != null)
                    GetSingleSeries(node, ref episodes);
            }

            return episodes.ToArray();
        }

        private void GetSingleSeries(HtmlNode node, ref List<Episode> episodes)
        {
            var link = Utils.Combine(Info().SiteUrl, node.Attributes["href"].Value);

            episodes.Add(new Episode()
            {
                Name = "Смотреть",
                MovieUrl = link,
                IsDirectLink = true
            });
        }

        private void GetMultipleSeries(HtmlNodeCollection nodes, ref List<Episode> episodes)
        {
            foreach (var node in nodes)
            {
                try
                {
                    var nameNode = node.SelectSingleNode(".//a/div/span");
                    var name = nameNode.InnerText;

                    var linkNode = node.SelectSingleNode(".//nav/a");
                    var link = Utils.Combine(Info().SiteUrl, linkNode.Attributes["href"].Value);

                    episodes.Add(new Episode()
                    {
                        Name = name,
                        MovieUrl = link,
                        IsDirectLink = true
                    });
                }
                catch (Exception ex)
                {
                    LogManager.GetCurrentClassLogger().Error(ex, "Ошибка при парсинге сериала");
                }
            }
        }
    }
}