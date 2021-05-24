using DiegoG.WebWatcher;
using HtmlAgilityPack;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.XPath;
using System.Threading.Tasks;
using System.Net.Http;

namespace UrbeWatcher
{
    [Watcher]
    public class InscriptionWatcher : IWebWatcher
    {
        private const long ChannelID = -1001188047483;
        private const string UrbeURL = "https://inscripciones.urbe.edu/InscripcionWeb/";
        public string Name => "InscriptionWatcher";

        public TimeSpan Interval { get; } = TimeSpan.FromMinutes(10);

        private static bool WasAvailable = false;
        public async Task Check()
        {
            Log.Information("Checking Urbe for availability");
            try
            {
                Log.Debug("Getting Web Response from " + UrbeURL);
                HttpClient client = new();
                var checkingResponse = await client.GetAsync(UrbeURL);

                if (checkingResponse.IsSuccessStatusCode && !WasAvailable)
                {
                    Log.Debug("Urbe is available");
                    WasAvailable = true;
                    OutputBot.SendTextMessage(ChannelID, $"URBE is Alive!!: {UrbeURL}\n{DateTime.Now:g}");
                }
                else
                {
                    WasAvailable = false;
                    Log.Debug("URBE is unavailable");
                }
            }
            catch (WebException)
            {
                Log.Error($"Couldn't Connect to {UrbeURL}. Aborting Task and trying again later");
            }
        }

        public Task FirstCheck() => Check();
    }
}
