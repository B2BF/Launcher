using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using System.Collections;

namespace B2BF.Common.Updater
{
    public class PhoenixUpdateXml
    {
        public Version GameVersion { get; private set; }
        public Boolean Hydra { get; private set; }
        public Dictionary<string, string> Files { get; private set; }
        public PhoenixUpdateXml(Version gameversion, Boolean hydra, Dictionary<string, string> files)
        {
            GameVersion = gameversion;
            Hydra = hydra;
            Files = files;
        }
        public bool IsNewerThan(Version version)
        {
            return this.GameVersion > version;
        }
        public static PhoenixUpdateXml Parse(Uri location)
        {
            Dictionary<string, string> list = new Dictionary<string, string>();
            PhoenixUpdateXml result;
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = ((object s, X509Certificate ce, X509Chain ch, SslPolicyErrors ssl) =>
                {
                    if (ce.GetSerialNumberString() == "")
                        return true;
                    return true;
                });
                using (var httpClient = new HttpClient())
                {
                    var updaterXmlData = httpClient.GetStringAsync(location).Result;
                    var updaterStruct = JsonConvert.DeserializeObject<UpdaterStruct>(updaterXmlData);
                    foreach (var file in updaterStruct.Files)
                        list.Add(file.Key, file.Value);//add to ignore case list
                    result = new PhoenixUpdateXml(updaterStruct.GameVersion, false, list);
                }
            }
            catch (Exception ex)
            {
                result = null;
            }
            return result;
        }

        public enum HashType
        {
            MD5,
            SHA512
        }

        private struct UpdaterStruct
        {
            [JsonProperty("Game")]
            public string GameId { get; set; }
            [JsonProperty("HashType")]
            public HashType HashType { get; set; }
            [JsonProperty("TotalUpdateSize")]
            public ulong TotalUpdateSize { get; set; }
            [JsonProperty("GameVersion")]
            public Version GameVersion { get; set; }
            [JsonProperty("Files")]
            public Dictionary<string, string> Files { get; set; }
            [JsonProperty("FileSizes")]
            public Dictionary<string, uint> FileSizes { get; set; }
            [JsonProperty("Deltas")]
            public List<PhoenixDeltaUpdate> Deltas { get; set; }
            [JsonProperty("Prerequisites")]
            public bool NeedsPrerequisites { get; set; }
        }

        public struct PhoenixDeltaUpdate
        {
            [JsonProperty("Id")]
            public string Id { get; set; }
            [JsonProperty("Files")]
            public Dictionary<string, string> Files { get; set; }
        }
    }
}