using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B2BF.Common.Models
{
    public class GameSpyServer
    {
        public string IPAddress { get; set; }
        public int QueryPort { get; set; }
        public Dictionary<string, string> ServerStats { get; set; }
    }
    public class PBF2Server : GameSpyServer
    {
        public bool Valid { get; set; }
        public DateTime LastRefreshed { get; set; }
        public DateTime LastPing { get; set; }
        public PBF2Server()
        {
            ServerStats = new Dictionary<string, string>();
        }
    }
}