using AnimeTube_Linux.Models;
using System.Collections.Generic;

namespace AnimeTube_Linux.Logic.Providers
{
    public interface IProvider
    {
        ProviderAttribute Info();
        Series[] SearchForSeries(string name);
        Episode[] SearchForEpisode(string url);
    }
}