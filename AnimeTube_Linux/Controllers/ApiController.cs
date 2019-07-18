using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AnimeTube_Linux.Logic;
using AnimeTube_Linux.Logic.Providers;
using AnimeTube_Linux.Models.Frontend;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AnimeTube_Linux.Controllers
{
    [Route("api")]
    public class ApiController : Controller
    {
        public Type[] GetProvidersType =>
            Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => typeof(IProvider).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract).ToArray();

        public IProvider GetProviderType(string name)
        {
            var type = Assembly.GetExecutingAssembly().GetTypes()
                .FirstOrDefault(t => typeof(IProvider).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract && t.Name == name);
            return (IProvider)type.GetMethod("GetInstance").Invoke(null, null);
        }

        [HttpGet, Route("providers")]
        public List<Provider> GetProviders()
        {
            var result = new List<Provider>();
            foreach(var provider in GetProvidersType)
            {
                var providerObj = GetProviderType(provider.Name);
                result.Add(new Provider()
                {
                    TechnicalName = provider.Name,
                    Name = providerObj.Info().Name,
                    URL = providerObj.Info().SiteUrl,
                    Favicon = providerObj.Info().FaviconUrl
                });
            }
            return result;
        }
        
        [HttpGet, Route("search")]
        public List<Series> SearchSeries(string providerName, string query)
        {
            var result = new List<Series>();

            var provider = GetProviderType(providerName);

            foreach(var series in provider.SearchForSeries(query))
            {
                result.Add(new Series()
                {
                    Title = series.Name,
                    Poster = series.ImageUrl,
                    Url = Url.Action("Watch", "Home", new { providerName, url = series.SeriesUrl, series = series.Name })
                });
            }

            return result;
        }

        [HttpGet, Route("episodes")]
        public List<Episode> SearchEpisodes(string providerName, string url)
        {
            var result = new List<Episode>();

            var provider = GetProviderType(providerName);

            foreach (var episode in provider.SearchForEpisode(url))
            {
                result.Add(new Episode()
                {
                    Name = episode.Name,
                    Link = episode.MovieUrl,
                    IsDirect = episode.IsDirectLink
                });
            }

            return result;
        }

        [HttpGet, Route("directlink")]
        public (string, string)[] GetDirectLink(string url)
        {
            return DirectLinkFinder.Find(url);
        }
    }
}
