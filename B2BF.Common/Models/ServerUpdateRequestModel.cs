using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B2BF.Common.Models
{
    public class ServerUpdateRequestModel
    {
        public ServerUpdateRequestServerInfoModel ServerInfo { get; set; } = new();

        public ServerUpdateRequestPlayerInfoModel PlayerInfo { get; set; } = new();

        public ServerUpdateRequestTeamInfoModel TeamInfo { get; set; } = new();
    }

    public class ServerUpdateRequestServerInfoModel
    {
        public bool NatNeg { get; set; }

        public string GameName { get; set; }

        public string HostName { get; set; }

        public string MapName { get; set; }

        public string GameType { get; set; }

        public string GameVariant { get; set; }

        public int NumPlayers { get; set; }

        public int MaxPlayers { get; set; }

        public bool Ranked { get; set; }

        public Dictionary<string, string> ServerData { get; set; } = new();
    }

    public class ServerUpdateRequestPlayerInfoModel
    {
        public List<Dictionary<string, object>> Players { get; set; } = new();
    }

    public class ServerUpdateRequestTeamInfoModel
    {

    }
}