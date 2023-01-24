using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;

    public static class SimpleHttpClient
    {
        public static string HttpGet(string url)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            //httpWebRequest.Timeout = 15000;
            //httpWebRequest.ReadWriteTimeout = 15000;
            httpWebRequest.Method = "Get";
            httpWebRequest.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
            using (var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                string Charset = httpWebResponse.CharacterSet;
                using (var receiveStream = httpWebResponse.GetResponseStream())
                using (var streamReader = new StreamReader(receiveStream, Encoding.GetEncoding(Charset)))
                    return streamReader.ReadToEnd();
            }
        }

        public static string HttpPost(string url, string data)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            //httpWebRequest.Timeout = 15000;
            //httpWebRequest.ReadWriteTimeout = 15000;
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            if (data != null)
            {
                httpWebRequest.ContentLength = data.Length;
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(data);
                }
            }
            using (var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                string Charset = httpWebResponse.CharacterSet;
                using (var receiveStream = httpWebResponse.GetResponseStream())
                using (var streamReader = new StreamReader(receiveStream, Encoding.GetEncoding(Charset)))
                    return streamReader.ReadToEnd();
            }
        }

        public static byte[] HttpGetBytes(string url)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            //httpWebRequest.Timeout = 15000;
            //httpWebRequest.ReadWriteTimeout = 15000;
            httpWebRequest.Method = "Get";
            httpWebRequest.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
            using (var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                using (var receiveStream = httpWebResponse.GetResponseStream())
                using (var memoryStream = new MemoryStream())
                {
                    receiveStream.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
        }

        public static byte[] HttpPostBytes(string url, byte[] data)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            //httpWebRequest.Timeout = 15000;
            //httpWebRequest.ReadWriteTimeout = 15000;
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            if (data != null)
            {
                httpWebRequest.ContentLength = data.Length;
                using (var writter = new BinaryWriter(httpWebRequest.GetRequestStream()))
                {
                    writter.Write(data);
                }
            }
            using (var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                using (var receiveStream = httpWebResponse.GetResponseStream())
                using (var memoryStream = new MemoryStream())
                {
                    receiveStream.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
        }

    }
