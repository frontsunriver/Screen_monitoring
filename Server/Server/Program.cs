// See https://aka.ms/new-console-template for more information

using EmbedIO;
using EmbedIO.WebApi;
using Swan;
using Swan.Logging;
using System.Diagnostics;

AppHelper.QuickEditMode(false);
//Console.BufferHeight = Int16.MaxValue - 1;
//AppHelper.MoveWindow(AppHelper.GetConsoleWindow(), 24, 0, 1080, 280, true);
AppHelper.FixCulture();

// See https://github.com/tonerdo/dotnet-env
if (!Debugger.IsAttached)
{
    //ConsoleLogger.Instance.LogLevel = LogLevel.Fatal;
    Swan.Logging.Logger.UnregisterLogger<ConsoleLogger>();
}
string url = "http://*:9932/";
using var server = CreateWebServer(url);
server.RunAsync();
Console.ReadKey(true);

/**
 * https://github.com/unosquare/embedio
 */
static WebServer CreateWebServer(string url)
{
    var server = new WebServer(o => o
            .WithUrlPrefix(url)
            .WithMode(HttpListenerMode.EmbedIO))
        .WithWebApi("/", m => m.WithController<V1Controller>());
    server.StateChanged += (s, e) => $"WebServer New State - {e.NewState}".Info();
    return server!;
}

