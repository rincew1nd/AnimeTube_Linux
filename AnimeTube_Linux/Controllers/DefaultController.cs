using AnimeTube_Linux.Logic.Providers;
using AnimeTube_Linux.Models;
using AnimeTube_Linux.Models.ForkModels;
using Microsoft.AspNetCore.Mvc;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AnimeTube_Linux.Controllers
{
    [Route("api")]
    public class DefaultController : ControllerBase
    {
        public Type[] GetProvidersType =>
            Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => typeof(IProvider).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract).ToArray();

        public IProvider GetProviderType(string name)
        {
            var type = Assembly.GetExecutingAssembly().GetTypes()
                .FirstOrDefault(t => typeof(IProvider).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract && t.Name == name);
            return (IProvider) type.GetMethod("GetInstance").Invoke(null, null);
        }

        [HttpGet, Route("", Name = "HomeLink")]
        public Item Index()
        {
            var menu = new Item()
            {
                PlaylistName = "Anime Tube",
                NextPageUrl = ""
            };
            
            foreach (var provider in GetProvidersType)
            {
                var providerObj = GetProviderType(provider.Name);
                menu.Channels.Add(new Channel()
                {
                    Title = provider.Name,
                    SearchOn = "Введите название аниме",
                    PlaylistUrl = Url.Link("SearchLink", new { Action = "search", ProviderName = provider.Name }),
                    Logo = providerObj.Info().FaviconUrl
                });
            }

            return menu;
        }

        [HttpGet, Route("search/{providerName}", Name = "SearchLink")]
        public Item Search(string providerName, string search)
        {
            var searchresult = new Item() { PlaylistName = "Результат поиска", NextPageUrl = "" };

            try
            {
                var provider = GetProviderType(providerName);
                var seriesFound = new List<Series>();// provider.SearchForSeries(search);

                foreach (var series in seriesFound)
                {
                    searchresult.Channels.Add(new Channel()
                    {
                        Title = series.Name,
                        PlaylistUrl = Url.Link("EpisodesLink", new { Action = "episodes", ProviderName = providerName, SeriesName = series.Name, url = series.SeriesUrl }),
                        Logo = series.ImageUrl,
                        Description = series.Description
                    });
                }

                if (!seriesFound.Any())
                {
                    searchresult.PlaylistName = "Ничего не найдено";
                    searchresult.Channels.Add(new Channel()
                    {
                        Title = "Вернуться на гравную",
                        PlaylistUrl = Url.Link("HomeLink", new { Action = "index" }),
                        Logo = provider.Info().FaviconUrl
                    });
                }
            }
            catch (Exception ex)
            {
                searchresult.Channels.Add(new Channel()
                {
                    Title = $"Произошла ошибка при поиске {search} в {providerName}",
                    Description = ex.Message
                });
                searchresult.Channels.Add(new Channel()
                {
                    Title = $"Обратитесь к разработчику с описанием ошибки",
                    Description = $"Письмо на E-Mail: rincew1nd@ya.ru с темой 'Ошибка AnimeTube'"
                });
                LogManager.GetCurrentClassLogger().Error(ex, $"Ошибка при поиске {search} в {providerName}");
            }

            return searchresult;
        }

        [HttpGet, Route("episodes/{providerName}", Name = "EpisodesLink")]
        public Item Episodes(string providerName, string seriesName, string url)
        {
            var item = new Item()
            {
                PlaylistName = seriesName
            };
            try
            {
                var provider = GetProviderType(providerName);

                var episodes = provider.SearchForEpisode(url);
                foreach (var episode in episodes.Where(ep => ep.IsDirectLink))
                {
                    item.Channels.Add(new Channel()
                    {
                        Title = episode.Name,
                        StreamUrl = episode.MovieUrl
                    });
                }
                foreach (var episode in episodes.Where(ep => !ep.IsDirectLink))
                {
                    string name = episode.Name;

                    if (url.Contains("stromo"))
                        name += " Stromo";
                    if (url.Contains("sibnet"))
                        name += " Siibnet";

                    item.Channels.Add(new Channel()
                    {
                        Title = episode.Name,
                        PlaylistUrl = Url.Link("DirectLink", new { Action = "index", series = seriesName, episode = episode.Name, url = episode.MovieUrl })
                    });
                }
            }
            catch (Exception ex)
            {
                item.Channels.Add(new Channel()
                {
                    Title = $"Произошла ошибка при поиске эпизодов {seriesName} в {providerName}",
                    Description = ex.Message
                });
                item.Channels.Add(new Channel()
                {
                    Title = $"Обратитесь к разработчику с описанием ошибки",
                    Description = $"Письмо на E-Mail: rincew1nd@ya.ru с темой 'Ошибка AnimeTube'"
                });
                LogManager.GetCurrentClassLogger().Error(ex, $"Ошибка при поиске эпизодов {seriesName} в {providerName}");
            }

            return item;
        }
    }
}