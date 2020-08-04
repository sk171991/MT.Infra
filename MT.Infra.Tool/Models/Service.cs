using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace MT.Infra.Tool.Models
{
    public class Service
    {

        public HttpClient Client { get; set; }
        public Service()
        {
            Client = new HttpClient();
            Client.BaseAddress = new Uri(ConfigurationManager.AppSettings["ServiceUrl"].ToString());
        }
        public HttpResponseMessage GetResponse(string url)
        {
            return Client.GetAsync(url).Result;
        }
    }   
}