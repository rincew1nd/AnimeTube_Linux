using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnimeTube_Linux.Models
{
    public class Episode
    {
        public string Name { get; set; }
        public string MovieUrl { get; set; }
        public bool IsDirectLink { get; set; }
    }
}