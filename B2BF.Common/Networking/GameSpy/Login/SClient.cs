using B2BF.Common.Account;
using B2BF.Common.Networking.GameSpy.Login.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace B2BF.Common.Networking.GameSpy.Login
{
    public class SClient
    {
        private Socket _clientSocket;

        private byte[] _buffer = new byte[8192];

        public SClient(Socket socket)
        {
            _clientSocket = socket;

            _clientSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, RecvCallback, null);
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
                    case "nicks":
                        Send(string.Format("\\nr\\1\\nick\\{0}\\uniquenick\\{1}\\ndone\\final\\", AccountInfo.Username, AccountInfo.Username));
                        break;
                    case "check":
                        Send(string.Format("\\cur\\0\\pid\\{0}\\final\\", AccountInfo.UId));
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