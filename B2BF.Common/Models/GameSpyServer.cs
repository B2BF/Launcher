using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B2BF.Common.Models
{
    public class GameSpyServer
    {
        [JsonIgnore]
        public string Hostname => ServerStats.ContainsKey("hostname") ? ServerStats["hostname"] : string.Empty;
        [JsonIgnore]
        public string GameType => ServerStats.ContainsKey("gametype") ? ServerStats["gametype"] : string.Empty;
        [JsonIgnore]
        public string MapName => ServerStats.ContainsKey("mapname") ? ServerStats["mapname"] : string.Empty;
        [JsonIgnore]
        public int NumPlayers => ServerStats.ContainsKey("numplayers") ? int.Parse(ServerStats["numplayers"]) : 0;
        [JsonIgnore]
        public int MaxPlayers => ServerStats.ContainsKey("maxplayers") ? int.Parse(ServerStats["maxplayers"]) : 0;
        [JsonIgnore]
        public int HostPort => ServerStats.ContainsKey("hostport") ? int.Parse(ServerStats["hostport"]) : 0;
        [JsonIgnore]
        public string GameVariant => ServerStats.ContainsKey("gamevariant") ? ServerStats["gamevariant"] : string.Empty;
        [JsonIgnore]
        public string GameMode => ServerStats.ContainsKey("gamemode") ? ServerStats["gamemode"] : string.Empty;
        [JsonIgnore]
        public bool Password => ServerStats.ContainsKey("password") ? int.Parse(ServerStats["password"]) == 1 : false;
        [JsonIgnore]
        public string GameVer => ServerStats.ContainsKey("gamever") ? ServerStats["gamever"] : string.Empty;
        [JsonIgnore]
        public string Bf2_Os => ServerStats.ContainsKey("bf2_os") ? ServerStats["bf2_os"] : string.Empty;
        [JsonIgnore]
        public bool Bf2_Anticheat => ServerStats.ContainsKey("bf2_anticheat") ? int.Parse(ServerStats["bf2_anticheat"]) == 1 : false;
        [JsonIgnore]
        public bool Bf2_Ranked => ServerStats.ContainsKey("bf2_ranked") ? int.Parse(ServerStats["bf2_ranked"]) == 1 : false;
        [JsonIgnore]
        public bool Bf2_Voip => ServerStats.ContainsKey("bf2_voip") ? int.Parse(ServerStats["bf2_voip"]) == 1 : false;
        [JsonIgnore]
        public bool Bf2_AutoRec => ServerStats.ContainsKey("bf2_autorec") ? int.Parse(ServerStats["bf2_autorec"]) == 1 : false;
        [JsonIgnore]
        public bool Bf2_Dedicated => ServerStats.ContainsKey("bf2_dedicated") ? int.Parse(ServerStats["bf2_dedicated"]) == 1 : false;
        [JsonIgnore]
        public bool Bf2_Pure => ServerStats.ContainsKey("bf2_pure") ? bool.Parse(ServerStats["bf2_pure"]) : false;
        [JsonIgnore]
        public int Bf2_Bots => ServerStats.ContainsKey("bf2_bots") ? int.Parse(ServerStats["bf2_bots"]) : 0;
        [JsonIgnore]
        public int Bf2_MapSize => ServerStats.ContainsKey("bf2_mapsize") ? int.Parse(ServerStats["bf2_mapsize"]) : 0;
        [JsonIgnore]
        public int Bf2_Fps => ServerStats.ContainsKey("bf2_fps") ? int.Parse(ServerStats["bf2_fps"]) : 0;
        [JsonIgnore]
        public bool Bf2_Plasma => ServerStats.ContainsKey("bf2_plasma") ? bool.Parse(ServerStats["bf2_plasma"]) : false;
        [JsonIgnore]
        public int Bf2_ReservedSlots => ServerStats.ContainsKey("bf2_reservedslots") ? int.Parse(ServerStats["bf2_reservedslots"]) : 0;

        public string IPAddress { get; set; }
        public int QueryPort { get; set; }
        public Dictionary<string, string> ServerStats { get; set; }
    }

    public class PBF2Server
    {
        public bool Valid { get; set; }
        public DateTime LastRefreshed { get; set; }
        public DateTime LastPing { get; set; }
        public PBF2Server()
        {
            
        }
    }
}