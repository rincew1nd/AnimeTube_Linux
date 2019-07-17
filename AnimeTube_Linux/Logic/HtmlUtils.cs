using HtmlAgilityPack;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace AnimeTube_Linux.Logic
{
    public static class HtmlUtils
    {
        public static string GetHtmlPage(string urlAddress, (string, string)[] headers = null, (string, string)[] formData = null, string referer = null, string useragent = null)
        {
            var request = MakeRequest(urlAddress, headers, formData, referer, useragent);

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null || response.CharacterSet == "")
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                string data = readStream.ReadToEnd();

                response.Close();
                readStream.Close();

                return data;
            }

            return null;
        }

        public static WebHeaderCollection GetHeadersPage(string urlAddress, (string, string)[] headers = null, (string, string)[] formData = null, string referer = null, string useragent = null)
        {
            var request = MakeRequest(urlAddress, headers, formData, referer, useragent);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            response.Headers.Add("ResponseUri", response.ResponseUri.AbsoluteUri);
            return response.Headers;
        }

        private static HttpWebRequest MakeRequest(string urlAddress, (string, string)[] headers = null, (string, string)[] formData = null, string referer = null, string useragent = null)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            if (headers != null && headers.Length > 0)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Item1, header.Item2);
                }
            }
            if (formData != null)
            {
                var formDataString = string.Join("&", formData.Select(fd => $"{fd.Item1}={Uri.EscapeUriString(fd.Item2)}"));
                var data = Encoding.UTF8.GetBytes(formDataString);

                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            if (!string.IsNullOrWhiteSpace(referer))
            {
                request.Referer = referer;
            }
            if (!string.IsNullOrWhiteSpace(useragent))
            {
                request.UserAgent = useragent;
            }
            return request;
        }

        public static HtmlDocument GetDocument(string url)
        {
            var doc = new HtmlDocument();
            try
            {
                doc.LoadHtml(GetHtmlPage(url));
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex, "Ошибка при получении страницы");
            }
            return doc;
        }
    }
}