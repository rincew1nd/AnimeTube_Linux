using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnimeTube_Linux.Models
{
    public class Series
    {
        public string Name { get; set; }
        public string SeriesUrl { get; internal set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
    }
}