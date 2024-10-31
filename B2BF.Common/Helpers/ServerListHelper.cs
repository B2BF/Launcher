using B2BF.Common.Account;
using B2BF.Common.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace B2BF.Common.Helpers
{
    public static class ServerListHelper
    {
        public static List<GameSpyServer> Servers { get; private set; } = new();

        private static System.Timers.Timer _timer = new System.Timers.Timer(30000);
        private static HttpClient _httpClient = new HttpClient();

        static ServerListHelper() 
        {
            _timer.Elapsed += _timer_Elapsed;
            _timer.AutoReset = true;
            _timer.Start();
        }

        public static void Start()
        {

        }

        private static async void _timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccountInfo.AccessToken);
                var str = await _httpClient.GetStringAsync("https://b2bf2.net/api/gamespy/servers");
                var serverList = JsonConvert.DeserializeObject<List<GameSpyServer>>(str);

                Servers = serverList;
            }
            catch (Exception ex)
            {

            }
        }
    }
}