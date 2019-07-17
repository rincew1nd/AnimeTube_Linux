using HtmlAgilityPack;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace AnimeTube_Linux.Logic
{
    public static class DirectLinkFinder
    {
        private static string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.157 Safari/537.36";

        public static (string, string)[] Find(string url)
        {
            var uri = new Uri(url);

            switch (uri.Host)
            {
                case "video.sibnet.ru":
                    return SibnetParser(url);
                case "www.stormo.tv":
                    return StromoParser(url);
                case "online.anidub.com":
                    return AnidubParser(url);
                case "anime.anidub.com":
                    return AnidubParser(url);
                case "sovetromantica.com":
                    return SovetRomantica(url);
                case "animedia.online":
                    return AnimediaOnline(url);
                default:
                    return null;
            }
        }

        private static (string, string)[] AnimediaOnline(string url)
        {
            if (url.Contains("#"))
            {
                var pos = url.IndexOf("#");
                url = url.Substring(0, pos);
            }

            var result = new List<(string, string)>();

            var doc = HtmlUtils.GetDocument(url);

            var videoNode = doc.DocumentNode.SelectSingleNode("//iframe[@id='video_white']");
            if (videoNode != null)
            {
                var videoLink = videoNode.Attributes["src"].Value;
                var playerDoc = HtmlUtils.GetHtmlPage(videoLink);

                var json = Regex.Match(playerDoc, "new Playerjs\\(({.+?})\\);", RegexOptions.Singleline).Groups[1].Value;
                dynamic jsonObj = JsonConvert.DeserializeObject(json);

                var playlist = HtmlUtils.GetHtmlPage((string)jsonObj.file);
                (string, string)[] tempPlaylist = ParseM3U8(playlist);

                foreach(var obj in tempPlaylist)
                {
                    string link = jsonObj.file;
                    var linkIndex = link.LastIndexOf('/');
                    link = link.Substring(0, linkIndex) + obj.Item2.Substring(1, obj.Item2.Length - 1);
                    result.Add((obj.Item1, link));
                }
            }

            return result.ToArray();
        }

        private static (string, string)[] SovetRomantica(string url)
        {
            var result = new List<(string, string)>();

            var doc = HtmlUtils.GetDocument(url);

            var linkNode = doc.DocumentNode.SelectSingleNode("//video/source");
            if (linkNode != null)
            {
                result.Add(("Смотреть", linkNode.Attributes["src"].Value));
            }

            return result.ToArray();
        }

        private static (string, string)[] AnidubParser(string url)
        {
            var result = new List<(string, string)>();

            var refererUrl = url.Replace("online.anidub", "anime.anidub");

            var newUrl = refererUrl.Replace("index.php", "video.php");
            var playlist1 = HtmlUtils.GetHtmlPage(newUrl, useragent: UserAgent, referer: refererUrl);
            result.AddRange(ParseM3U8(playlist1));

            newUrl = refererUrl.Replace("index.php", "noad.php");
            var playlist2 = HtmlUtils.GetHtmlPage(newUrl, useragent: UserAgent, referer: refererUrl);
            result.AddRange(ParseM3U8(playlist2));

            return result.ToArray();
        }

        private static (string, string)[] StromoParser(string url)
        {
            var doc = HtmlUtils.GetHtmlPage(url);

            var javascriptBlock = Regex.Match(doc, "new Playerjs\\(({.+?})\\);", RegexOptions.Singleline)?.Groups[1].Value;
            dynamic jsonObj = JsonConvert.DeserializeObject(javascriptBlock);
            var files = ((string)jsonObj.file).Replace("/,", string.Empty).Split(new[] { ']', '[' }, StringSplitOptions.RemoveEmptyEntries);

            var result = new List<(string, string)>();

            if (files.Length == 1)
            {
                result.Add(("Default", files[0]));
            }
            else
            {
                for (var i = 0; i < files.Length; i += 2)
                    result.Add((files[i], files[i + 1]));
            }
            return result.ToArray();
        }

        private static (string, string)[] SibnetParser(string url)
        {
            var requestUrl = $"https://video.sibnet.ru/video{Regex.Match(url, "(\\d{2,})").Groups[1].Value}/";
            var requestHeaders = HtmlUtils.GetHeadersPage(requestUrl);
            var location = requestHeaders["ResponseUri"];

            var doc = HtmlUtils.GetHtmlPage(location, null, new []{ ("buffer_method", "full") });

            var requestLink = "https://video.sibnet.ru" + Regex.Match(doc, "([^\"]+.(m3u8|mp4))")?.Groups[1].Value;
            var movieLocation = HtmlUtils.GetHeadersPage(requestLink, referer: location)["ResponseUri"];

            return new []{ ("Смотреть", movieLocation) };
        }

        private static (string, string)[] ParseM3U8(string playlist)
        {
            var result = new List<(string, string)>();

            var lines = playlist.Split('\n');
            for(var i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("#EXT-X"))
                {
                    result.Add((lines[i], lines[++i]));
                }
            }

            return result.ToArray();
        }
    }
}