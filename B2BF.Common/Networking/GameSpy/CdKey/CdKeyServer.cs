using B2BF.Common.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using B2BF.Common.Helpers;

namespace B2BF.Common.Networking.GameSpy.CdKey
{
    public static class CdKeyServer
    {
        private static Socket _serverReportSocket;
        private static byte[] _buffer = new byte[8092];

        static CdKeyServer()
        {
            _serverReportSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _serverReportSocket.Bind(new IPEndPoint(IPAddress.Loopback, 29910));
        }

        public static void Start()
        {
            EndPoint ep = new IPEndPoint(IPAddress.Any, 0);
            _serverReportSocket.BeginReceiveFrom(_buffer, 0, _buffer.Length, SocketFlags.None, ref ep, ServerReportCallback, null);
        }

        private static void ServerReportCallback(IAsyncResult ar)
        {
            EndPoint ep = new IPEndPoint(IPAddress.Any, 0);
            var transferred = _serverReportSocket.EndReceiveFrom(ar, ref ep);

            var bytes = new byte[transferred];
            Array.Copy(_buffer, bytes, transferred);

            Debug.WriteLine($"Received CdKey: {bytes[0]}");

            var raw = Encoding.UTF8.GetString(bytes);
            if (raw.StartsWith("\\auth\\"))
            {
                raw = raw.Trim('\\');
                var kv = GameSpyHelper.ConvertToKeyValue(raw.Split('\\'));
                var resp = "\\uok\\\\cd\\{0}\\skey\\{1}";
                resp = string.Format(resp, kv["resp"], kv["skey"]);
                _serverReportSocket.SendTo(Encoding.UTF8.GetBytes(resp), ep);
            }

        skip:
            EndPoint ep2 = new IPEndPoint(IPAddress.Any, 0);
            _serverReportSocket.BeginReceiveFrom(_buffer, 0, _buffer.Length, SocketFlags.None, ref ep2, ServerReportCallback, null);
        }
    }
}