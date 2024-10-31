using B2BF.Common.Helpers;
using B2BF.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace B2BF.Common.Networking.GameSpy.Search
{
    public class SearchClient
    {
        private Socket _clientSocket;

        private byte[] _buffer = new byte[8192];

        public SearchClient(Socket socket)
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
                using (var mem = new MemoryStream(bytes))
                using (var br = new BinaryReader(mem))
                {
                    br.BaseStream.Position += 3;
                    var listVersion = br.ReadByte();
                    var encodingVersion = br.ReadByte();
                    var gameVersion = br.ReadUInt32();
                    string queryGame = "";
                    byte b;
                    while ((b = br.ReadByte()) != 0x00)
                        queryGame += Convert.ToChar(b);
                    string gameName = "";
                    while ((b = br.ReadByte()) != 0x00)
                        gameName += Convert.ToChar(b);
                    if (queryGame == "battlefield2")
                        ParseRequest(gameName, br);
                }
            }
            catch (Exception ex)
            {

            }

            _clientSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, RecvCallback, null);
        }

        private bool ParseRequest(string gamename, BinaryReader br)
        {
            List<GameSpyServer> servers = ServerListHelper.Servers;
            string validate = "";
            byte p;
            while ((p = br.ReadByte()) != 0x00)
                validate += Convert.ToChar(p);
            validate += "\x00";
            string filter = "";
            byte b;
            while ((b = br.ReadByte()) != 0x00)
                filter += Convert.ToChar(b);
            string fieldList = "";
            while ((b = br.ReadByte()) != 0x00)
                fieldList += Convert.ToChar(b);
            var fields = filter.Split("\\");
            // http://aluigi.altervista.org/papers/gslist.cfg
            byte[] key;
            if (gamename == "battlefield2")
                key = Encoding.GetEncoding("ISO-8859-1").GetBytes("hW6m9a");
            else
                key = Encoding.GetEncoding("ISO-8859-1").GetBytes("Xn221z");
            if (servers == null)
                return false;
            byte[] unencryptedServerList = GameSpyHelper.PackServerList((IPEndPoint)_clientSocket.RemoteEndPoint, servers, fields);
            byte[] encryptedServerList = GameSpyHelper.GameSpyEncoding.Encode(key, Encoding.GetEncoding("ISO-8859-1").GetBytes(validate), unencryptedServerList, unencryptedServerList.LongLength);
            Send(encryptedServerList);
            return true;
        }

        public void Send(byte[] data)
        {
            _clientSocket.Send(data);
        }
    }
}