using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnimeTube_Linux.Logic.Providers
{
    public class ProviderAttribute : Attribute
    {
        public string Name;
        public string SiteUrl;
        public string FaviconUrl;

        /// <summary>
        /// Provider attribute for aditional info
        /// </summary>
        /// <param name="name">Provider name</param>
        /// <param name="site">Provider site url</param>
        /// <param name="favicon">Provider favicon path</param>
        public ProviderAttribute(string name, string site, string favicon)
        {
            Name = name;
            SiteUrl = site;
            FaviconUrl = favicon;
        }
    }
}