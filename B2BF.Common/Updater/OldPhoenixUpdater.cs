using B2BF.Common.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;

namespace B2BF.Common.Updater
{
    public class OldPhoenixUpdater
    {
        public Action<string> NotifyAction { get; set; }
        public Action<double, double> ProgressBarAction { get; set; }
        public Action<bool> StartButtonAction { get; set; }
        private PhoenixUpdateXml UpdateXML { get; set; }
        private WebClient _wClient { get; set; }
        private string GamePath { get; set; }
        private Dictionary<string, string> Redownload { get; set; }
        private string FileDownloading { get; set; }
        private int SameFile { get; set; }

        public OldPhoenixUpdater()
        {
            Redownload = new Dictionary<string, string>();
        }

        public void Start()
        {
            GamePath = Settings.BF2GamePath;
            var remoteUrl = "https://cdn.phoenixnetwork.net/updater/game-bf2.json";
            UpdateXML = PhoenixUpdateXml.Parse(new Uri(remoteUrl));
            var gameVersion = Settings.GetGameVersion("Battlefield2");
            if (gameVersion == null)
            {
                BeginUpdating(true);
            }
            else if (UpdateXML.IsNewerThan(gameVersion))
            {
                BeginUpdating();
            }
        skip: SetNotify("Finished", Color.Green);
            if (StartButtonAction != null)
            {
                StartButtonAction(true);
            }
        }
        private void BeginUpdating(bool full = false)
        {
            SetNotify("Downloading update...", Color.Orange);
            var remoteUrl = "https://cdn.phoenixnetwork.net/updater/versions/client/Battlefield2/" + Convert.ToString(UpdateXML.GameVersion) + "/";
            if (full)
            {
                if (!Directory.Exists(Path.Combine(Settings.BF2GamePath)))
                    Directory.CreateDirectory(Path.Combine(Settings.BF2GamePath));
                _wClient = new WebClient();
                try
                {
                    _wClient.DownloadProgressChanged += _wClient_DownloadProgressChanged;
                    foreach (var file in UpdateXML.Files)
                    {
                        var path = Path.Combine(GamePath, Path.GetDirectoryName(file.Key));
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);
                        if (!File.Exists(Path.Combine(GamePath, file.Key)))
                        {
                            FileDownloading = file.Key;
                            try
                            {
                                _wClient.DownloadFileTaskAsync(remoteUrl + file.Key.Replace(@"\", "/"), Path.Combine(GamePath, file.Key)).Wait();
                                using (var sha = new HMACMD5(Encoding.UTF8.GetBytes(file.Key)))
                                using (var fs = new FileStream(Path.Combine(GamePath, file.Key), FileMode.Open))
                                {
                                    if (!file.Value.Equals(ByteArrayToString(sha.ComputeHash(fs))))
                                        Redownload.Add(file.Key, file.Value);
                                }
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        else
                        {
                            using (var sha = new HMACMD5(Encoding.UTF8.GetBytes(file.Key)))
                            using (var fs = new FileStream(Path.Combine(GamePath, file.Key), FileMode.Open))
                            {
                                if (!file.Value.Equals(ByteArrayToString(sha.ComputeHash(fs))))
                                    Redownload.Add(file.Key, file.Value);
                                try
                                {
                                    File.Delete(Path.Combine(GamePath, file.Key));
                                }
                                catch (Exception)
                                {

                                }
                            }
                        }
                    }
                    do
                    {
                        try
                        {
                            var file = Redownload.Take(1).First();
                            Redownload.Remove(file.Key);
                            var path = Path.Combine(GamePath, Path.GetDirectoryName(file.Key));
                            if (!Directory.Exists(path))
                                Directory.CreateDirectory(path);
                            if (file.Key == FileDownloading)
                                SameFile++;
                            FileDownloading = file.Key;
                            _wClient.DownloadFileTaskAsync(remoteUrl + file.Key.Replace(@"\", "/"), Path.Combine(GamePath, file.Key)).Wait();
                            using (var sha = new HMACMD5(Encoding.UTF8.GetBytes(file.Key)))
                            using (var fs = new FileStream(Path.Combine(GamePath, file.Key), FileMode.Open))
                            {
                                if (!file.Value.Equals(ByteArrayToString(sha.ComputeHash(fs))))
                                {
                                    if (SameFile < 3)
                                        Redownload.Add(file.Key, file.Value);
                                }
                            }
                        }
                        catch (Exception)
                        {

                        }
                    }
                    while (Redownload.Count() != 0);
                }
                catch (Exception ex)
                {

                }
            }
            else
            {
                _wClient = new WebClient();
                try
                {
                    _wClient.DownloadProgressChanged += _wClient_DownloadProgressChanged;
                    foreach (var file in UpdateXML.Files)
                    {
                        if (File.Exists(Path.Combine(GamePath, file.Key)))
                        {
                            using (var sha = new HMACMD5(Encoding.UTF8.GetBytes(file.Key)))
                            {
                                if (!file.Value.Equals(ByteArrayToString(sha.ComputeHash(new FileStream(Path.Combine(GamePath, file.Key), FileMode.Open)))))
                                    Redownload.Add(file.Key, file.Value);
                            }
                            try
                            {
                                File.Delete(Path.Combine(GamePath, file.Key));
                            }
                            catch (Exception)
                            {

                            }
                        }
                        else
                        {
                            var path = Path.Combine(GamePath, Path.GetDirectoryName(file.Key));
                            if (!Directory.Exists(path))
                                Directory.CreateDirectory(path);
                            FileDownloading = file.Key;
                            try
                            {
                                _wClient.DownloadFileTaskAsync(remoteUrl + file.Key.Replace(@"\", "/"), Path.Combine(GamePath, file.Key)).Wait();
                                using (var sha = new HMACMD5(Encoding.UTF8.GetBytes(file.Key)))
                                using (var fs = new FileStream(Path.Combine(GamePath, file.Key), FileMode.Open))
                                {
                                    if (!file.Value.Equals(ByteArrayToString(sha.ComputeHash(fs))))
                                        Redownload.Add(file.Key, file.Value);
                                }
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                    }
                    do
                    {
                        try
                        {
                            var file = Redownload.Take(1).First();
                            Redownload.Remove(file.Key);
                            var path = Path.Combine(GamePath, Path.GetDirectoryName(file.Key));
                            if (!Directory.Exists(path))
                                Directory.CreateDirectory(path);
                            if (file.Key == FileDownloading)
                                SameFile++;
                            FileDownloading = file.Key;
                            _wClient.DownloadFileTaskAsync(remoteUrl + file.Key.Replace(@"\", "/"), Path.Combine(GamePath, file.Key)).Wait();
                            using (var sha = new HMACMD5(Encoding.UTF8.GetBytes(file.Key)))
                            using (var fs = new FileStream(Path.Combine(GamePath, file.Key), FileMode.Open))
                            {
                                if (!file.Value.Equals(ByteArrayToString(sha.ComputeHash(fs))))
                                {
                                    if (SameFile < 3)
                                        Redownload.Add(file.Key, file.Value);
                                }
                            }
                        }
                        catch (Exception)
                        {

                        }
                    }
                    while (Redownload.Count() != 0);
                }
                catch (Exception)
                {

                }
            }
            try
            {
                File.Delete(Path.Combine(Settings.BF2GamePath, "version.txt"));
                File.WriteAllText(Path.Combine(Settings.BF2GamePath, "version.txt"), UpdateXML.GameVersion.ToString());
            }
            catch (Exception)
            {

            }
            try
            {

            }
            catch (Exception)
            {
                File.WriteAllText(Path.Combine(Settings.GamePath, "version.txt"), UpdateXML.GameVersion.ToString());
            }
        }

        private void _wClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            try
            {
                if (e.TotalBytesToReceive > 0)
                {
                    NotifyAction("Downloading " + FileDownloading + " received: " + e.BytesReceived + " bytes of the " + e.BytesReceived + 1);
                    ProgressBarAction(e.TotalBytesToReceive, e.BytesReceived);
                }
                else
                {
                    NotifyAction("Downloading " + FileDownloading + " received: " + e.BytesReceived + " bytes of the " + e.BytesReceived + 1);
                    ProgressBarAction(e.BytesReceived + 1, e.BytesReceived);
                }
            }
            catch (Exception)
            {

            }
        }
        private void SetNotify(string text, Color color)
        {
            NotifyAction(text);
        }
        private static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

    }
}