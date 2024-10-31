using B2BF.Common.Networking.GameSpy.Login.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace B2BF.Common.Networking.GameSpy.Login
{
    public class LoginClient
    {
        public const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public string serverChallengeKey { get; set; }
        public string clientChallengeKey { get; set; }
        public string LoginToken { get; set; }

        private Socket _clientSocket;

        private byte[] _buffer = new byte[8192];

        public LoginClient(Socket socket)
        {
            _clientSocket = socket;

            string buffer = "";
            var rand = new Random((int)DateTime.Now.Ticks);
            for (int i = 0; i < 10; i++)
                buffer += chars[rand.Next(chars.Length)];
            serverChallengeKey = buffer;

            _clientSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, RecvCallback, null);

            Send(string.Format("\\lc\\1\\challenge\\{0}\\id\\1\\final\\", serverChallengeKey));
        }

        private void RecvCallback(IAsyncResult ar)
        {
            int transferred = 0;
            try
            {
                transferred = _clientSocket.EndReceive(ar);
                if (transferred <= 0)
                    throw new Exception();
            }
            catch (Exception ex)
            {
                return;
            }

            var bytes = new byte[transferred];
            Array.Copy(_buffer, bytes, transferred);

            try
            {
                var strData = Encoding.ASCII.GetString(bytes);
                var packetData = strData.Split('\\');
                switch (packetData[1])
                {
                    case "newuser":
                        Send("\\error\\err\\516\\fatal\\errmsg\\I dont know how you got here but dont do that. Login instead.\\id\\1\\final\\");
                        break;
                    case "login":
                        LoginPacket.Handle(packetData, this);
                        break;
                    case "getprofile":
                        GetProfilePacket.Handle(packetData, this);
                        break;
                    case "updatepro":
                        break;
                    case "logout":
                        break;
                }
            }
            catch (Exception ex)
            {

            }

            _clientSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, RecvCallback, null);
        }

        public void Send(string data)
        {
            _clientSocket.Send(Encoding.ASCII.GetBytes(data));
        }
    }
}