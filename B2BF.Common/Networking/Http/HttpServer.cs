using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Resources;
using System.Text;
using B2BF.Common.Account;

namespace B2BF.Common.Networking.Http
{
    public class HttpServer
    {
        public static readonly object _sync = new object();
        public static bool _exit;
        public static TcpListener lMagma = null;
        private static WebClient _wc = new WebClient();

        static HttpServer()
        {
            
        }

        public static void Start()
        {
            SetExit(false);
            new Thread(tHTTPMain).Start();
        }

        public static void Stop()
        {
            if (lMagma != null) lMagma.Stop();
            SetExit(true);
        }

        public static void tHTTPMain(object obj)
        {
            try
            {
#if DEBUG
                lMagma = new TcpListener(IPAddress.Any, 8888);
#else
                lMagma = new TcpListener(IPAddress.Loopback, 8888);
#endif
                lMagma.Start();
                TcpClient client;
                while (!GetExit())
                {
                    client = lMagma.AcceptTcpClient();
                    NetworkStream ns = client.GetStream();
                    byte[] data = ReadContentTCP(ns);
                    try
                    {
                        ProcessMagma(Encoding.ASCII.GetString(data), ns);
                    }
                    catch { }
                    client.Close();
                }
            }
            catch (Exception ex)
            {

            }
        }

        public static void ProcessMagma(string data, Stream s)
        {
            string[] lines = data.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            string cmd = lines[0].Split(' ')[0];
            string url = lines[0].Split(' ')[1].Split(':')[0];

            if (url.StartsWith("/oauthloginreturn"))
            {
                ReplyWithXML(s, "You may now close this tab.");
                var code = url.Substring(23, 32);
                var state = url.Substring(62);
                AccountInfo.ValidateLoginResult(code, state);
                return;
            }

            if (url.StartsWith("/ASP"))
                url = url.Substring(4);

            switch (cmd)
            {
                case "GET":
                    var response = _wc.DownloadString("https://stats.b2bf2.net" + url);
                    ReplyWithXML(s, response);
                    return;
                case "POST":
                    if (url.StartsWith("/selectunlock.aspx"))
                    {
                        var postResponse = _wc.UploadString("https://stats.b2bf2.net" + url, "");
                        ReplyWithXML(s, postResponse);
                        return;
                    }
                    _wc.Headers.Add("user-agent", "GameSpyHTTP/1.0");
                    var length = lines.Where(x => x.StartsWith("Content-Length"));
                    var idx = data.IndexOf('{');
                    var snapshot = data.Substring(idx);
                    _wc.UploadString("https://stats.b2bf2.net/bf2statistics.php", snapshot);
                    ReplyWithXML(s, "O");
                    break;
            }

            return;
        }

        public static void ReplyWithXML(Stream s, string c)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("HTTP/1.1 200 OK\r\n");
            sb.Append("Date: " + DateTime.Now.ToUniversalTime().ToString("r") + "\r\n");
            sb.Append("Server: Warranty Voiders\r\n");
            sb.Append("Content-Length: " + c.Length + "\r\n");
            sb.Append("Keep-Alive: timeout=5, max=100\r\n");
            sb.Append("Connection: Keep-Alive\r\n");
            sb.Append("\r\n");
            sb.Append(c);
            byte[] buf = Encoding.ASCII.GetBytes(sb.ToString());
            s.Write(buf, 0, buf.Length);
        }

        public static void SetExit(bool state)
        {
            lock (_sync)
            {
                _exit = state;
            }
        }

        public static bool GetExit()
        {
            bool result;
            lock (_sync)
            {
                result = _exit;
            }
            return result;
        }

        public static byte[] ReadContentTCP(NetworkStream Stream)
        {
            MemoryStream res = new MemoryStream();
            byte[] buff = new byte[0x10000];
            Stream.ReadTimeout = 100;
            int bytesRead;
            try
            {
                while ((bytesRead = Stream.Read(buff, 0, 0x10000)) > 0)
                    res.Write(buff, 0, bytesRead);
            }
            catch { }
            Stream.Flush();
            return res.ToArray();
        }
    }
}