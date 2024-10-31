using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace B2BF.Common.Networking.GameSpy.Login
{
    public static class LoginServer
    {
        private static Socket _listenSocket;
        private static Socket _listesSearchSocket;

        static LoginServer()
        {
            _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listenSocket.Bind(new IPEndPoint(IPAddress.Loopback, 29900));

            _listesSearchSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listesSearchSocket.Bind(new IPEndPoint(IPAddress.Loopback, 29901));
        }

        public static void Start()
        {
            _listenSocket.Listen(15);
            _listenSocket.BeginAccept(AcceptCallback, null);

            _listesSearchSocket.Listen(15);
            _listesSearchSocket.BeginAccept(AccepSearchCallback, null);
        }

        private static void AcceptCallback(IAsyncResult ar)
        {
            var socket = _listenSocket.EndAccept(ar);
            var client = new LoginClient(socket);

            _listenSocket.BeginAccept(AcceptCallback, null);
        }

        private static void AccepSearchCallback(IAsyncResult ar)
        {
            var socket = _listesSearchSocket.EndAccept(ar);
            var client = new SClient(socket);

            _listesSearchSocket.BeginAccept(AccepSearchCallback, null);
        }
    }
}