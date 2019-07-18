using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AnimeTube_Linux.Logic.Providers;
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

        [HttpGet]
        public List<(int, string)> GetProviders()
        {
            var result = new List<(int, string)>();
            var providers = GetProvidersType;
            for(var i = 0; i < providers.Length; i++)
            {
                result.Add((i, providers[i].Name));
            }
            return result;
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
