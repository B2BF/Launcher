using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace B2BF.Common.Networking.GameSpy.Search
{
    public static class SearchServer
    {
        private static Socket _listenSocket;

        static SearchServer()
        {
            _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
#if DEBUG
            _listenSocket.Bind(new IPEndPoint(IPAddress.Any, 28910));
#else
            _listenSocket.Bind(new IPEndPoint(IPAddress.Loopback, 28910));
#endif
        }

        public static void Start()
        {
            _listenSocket.Listen(15);
            _listenSocket.BeginAccept(AcceptCallback, null);
        }

        private static void AcceptCallback(IAsyncResult ar)
        {
            var socket = _listenSocket.EndAccept(ar);
            var client = new SearchClient(socket);

            _listenSocket.BeginAccept(AcceptCallback, null);
        }
    }
}