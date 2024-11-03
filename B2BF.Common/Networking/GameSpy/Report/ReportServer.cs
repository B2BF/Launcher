using B2BF.Common.Extensions;
using B2BF.Common.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace B2BF.Common.Networking.GameSpy.Report
{
	public static class ReportServer
	{
		private static Socket _serverReportSocket;
		private static byte[] _buffer = new byte[8092];

		static ReportServer()
		{
			_serverReportSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			_serverReportSocket.Bind(new IPEndPoint(IPAddress.Any, 27900));
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
			var ipep = ep as IPEndPoint;

			var bytes = new byte[transferred];
			Array.Copy(_buffer, bytes, transferred);

			if (bytes[0] == 0x09) // Hello I'm new (prequery ip verify)
			{
				_serverReportSocket.SendTo(new byte[] { 0xfe, 0xfd, 0x09, 0x00, 0x00, 0x00, 0x00 }, ep);
			}
			else if (bytes[0] == 0x03) // heartbeat
			{
				
			}
			else if (bytes[0] == 0x01) // challenge
			{

			}
			else if (bytes[0] == 0x08) // keep alive
			{
				
			}
			else
			{

			}
		skip:
			EndPoint ep2 = new IPEndPoint(IPAddress.Any, 0);
			_serverReportSocket.BeginReceiveFrom(_buffer, 0, _buffer.Length, SocketFlags.None, ref ep2, ServerReportCallback, null);
		}
	}
}