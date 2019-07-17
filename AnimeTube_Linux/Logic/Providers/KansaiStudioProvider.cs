using AnimeTube_Linux.Models;
using HtmlAgilityPack;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace AnimeTube_Linux.Logic.Providers
{
    [Provider("KansaiStudio", "https://kansai.studio", "https://kansai.studio//favicon.ico")]
    public class KansaiStudioProvider //: IProvider
    {
        private static KansaiStudioProvider _instance;
        private static object _lock = new object();
        public static KansaiStudioProvider GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new KansaiStudioProvider();
                    }
                }
            }
            return _instance;
        }

        private List<Series> _series = new List<Series>();

        public ProviderAttribute Info() => typeof(KansaiStudioProvider).GetCustomAttribute<ProviderAttribute>(false);

        private KansaiStudioProvider()
        {
            var pageNumber = 1;
            while (true)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(HtmlUtils.GetHtmlPage("https://kansai.studio/" + pageNumber));

                var movieNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'flatpost__container')]");
                if (movieNodes == null)
                    break;

                foreach (var movieNode in movieNodes)
                {
                    try
                    {
                        var image = movieNode.SelectSingleNode(".//div[contains(@class, 'flatpost__image')]").Attributes["style"].Value;
                        var imageUrl = Regex.Match(image, "background-image:url\\('(.+)'\\);").Groups[1].Value;
                        var titleNode = movieNode.SelectSingleNode(".//a[contains(@class, 'flatpost__title')]");
                        var title = titleNode.InnerText.Split('[')[0].TrimEnd(' ');
                        var link = "https://kansai.studio" + titleNode.Attributes["href"].Value;
                        var description = movieNode.SelectSingleNode(".//div[contains(@class, 'flatpost__description')]").InnerText;

                        _series.Add(new Series()
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

                pageNumber++;
            }
        }

        public Series[] SearchForSeries(string name)
        {
            return _series.Where(s => s.Name.Contains(name)).ToArray();
        }

        public Episode[] SearchForEpisode(string url)
        {
            var episodes = new List<Episode>();

            try
            {
                var htmlPage = HtmlUtils.GetHtmlPage(url);
                var javascriptBlock = Regex.Match(htmlPage, "\"series\":\\[([^\\]]+)\\]")?.Groups[1].Value;
                javascriptBlock = Regex.Replace(javascriptBlock, "\"(\\d+)\"", m => $"\"res{m.Groups[1].Value}\"");
                
                dynamic jsonResponse = JsonConvert.DeserializeObject($"{{\"seria\":[{javascriptBlock}]}}");
                foreach (var seria in jsonResponse.seria)
                {
                    if (!string.IsNullOrWhiteSpace(seria.links.res720?.ToString()))
                        episodes.Add(new Episode()
                        {
                            Name = $"Серия {seria.number} (720)",
                            MovieUrl = seria.links.res720,
                            IsDirectLink = true
                        });
                    if (!string.IsNullOrWhiteSpace(seria.links.res1080?.ToString()))
                        episodes.Add(new Episode()
                        {
                            Name = $"Серия {seria.number} (1080)",
                            MovieUrl = seria.links.res1080,
                            IsDirectLink = true
                        });
                }
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex);
            }

            return episodes.ToArray();
        }
    }
}