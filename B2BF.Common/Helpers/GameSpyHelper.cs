using B2BF.Common.Extensions;
using B2BF.Common.Models;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace B2BF.Common.Helpers
{
    public class GameSpyHelper
    {
        public static byte[] PackServerList(IPEndPoint remoteEndPoint, IEnumerable<GameSpyServer> servers, string[] fields)
        {
            byte[] ipBytes = remoteEndPoint.Address.GetAddressBytes();
            byte[] value2 = BitConverter.GetBytes((ushort)6500);
            byte fieldsCount = (byte)fields.Length;

            List<byte> data = new List<byte>();
            data.AddRange(ipBytes);
            data.AddRange(BitConverter.IsLittleEndian ? value2.Reverse() : value2);
            data.Add(fieldsCount);
            data.Add(0);

            foreach (var field in fields)
            {
                data.AddRange(Encoding.GetEncoding("ISO-8859-1").GetBytes(field));
                data.AddRange(new byte[] { 0, 0 });
            }

            foreach (var server in servers)
            {
                // commented this stuff out since it caused some issues on testing, might come back to it later and see what's happening...
                // NAT traversal stuff...
                // 126 (\x7E)	= public ip / public port / private ip / private port / icmp ip
                // 115 (\x73)	= public ip / public port / private ip / private port
                // 85 (\x55)	= public ip / public port
                // 81 (\x51)	= public ip / public port
                /*Console.WriteLine(server.IPAddress);
				Console.WriteLine(server.QueryPort);
				Console.WriteLine(server.localip0);
				Console.WriteLine(server.localip1);
				Console.WriteLine(server.localport);
				Console.WriteLine(server.natneg);*/
                if (!String.IsNullOrWhiteSpace(server.ServerStats["localip0"]) && !String.IsNullOrWhiteSpace(GetField(server, "localip1")) && Convert.ToInt32(server.ServerStats["localport"]) > 0)
                {
                    data.Add(126);
                    data.AddRange(IPAddress.Parse(server.IPAddress).GetAddressBytes());
                    data.AddRange(BitConverter.IsLittleEndian ? BitConverter.GetBytes((ushort)server.QueryPort).Reverse() : BitConverter.GetBytes((ushort)server.QueryPort));
                    data.AddRange(IPAddress.Parse(server.ServerStats["localip0"]).GetAddressBytes());
                    data.AddRange(BitConverter.IsLittleEndian ? BitConverter.GetBytes((ushort)Convert.ToInt32(server.ServerStats["localport"])).Reverse() : BitConverter.GetBytes((ushort)Convert.ToInt32(server.ServerStats["localport"])));
                    data.AddRange(IPAddress.Parse(server.ServerStats["localip1"]).GetAddressBytes());
                }
                else if (!String.IsNullOrWhiteSpace(server.ServerStats["localip0"]) && Convert.ToInt32(server.ServerStats["localport"]) > 0)
                {
                    data.Add(115);
                    data.AddRange(IPAddress.Parse(server.IPAddress).GetAddressBytes());
                    data.AddRange(BitConverter.IsLittleEndian ? BitConverter.GetBytes((ushort)server.QueryPort).Reverse() : BitConverter.GetBytes((ushort)server.QueryPort));
                    data.AddRange(IPAddress.Parse(server.ServerStats["localip0"]).GetAddressBytes());
                    data.AddRange(BitConverter.IsLittleEndian ? BitConverter.GetBytes((ushort)Convert.ToInt32(server.ServerStats["localport"])).Reverse() : BitConverter.GetBytes((ushort)Convert.ToInt32(server.ServerStats["localport"])));
                }
                else
                {
                    data.Add(81); // it could be 85 as well, unsure of the difference, but 81 seems more common...
                    data.AddRange(IPAddress.Parse(server.IPAddress).GetAddressBytes());
                    data.AddRange(BitConverter.IsLittleEndian ? BitConverter.GetBytes((ushort)server.QueryPort).Reverse() : BitConverter.GetBytes((ushort)server.QueryPort));
                }

                data.Add(255);

                for (int i = 0; i < fields.Length; i++)
                {
                    data.AddRange(Encoding.GetEncoding("ISO-8859-1").GetBytes(GetField(server, fields[i])));

                    if (i < fields.Length - 1)
                        data.AddRange(new byte[] { 0, 255 });
                }

                data.Add(0);
            }

            data.AddRange(new byte[] { 0, 255, 255, 255, 255 });

            return data.ToArray();
        }
        private static string GetField(GameSpyServer server, string fieldName)
        {
            string result = "";
            if (!server.ServerStats.TryGetValue(fieldName, out result))
                return string.Empty;
            return result;
        }
        public static Dictionary<string, string> ConvertToKeyValue(string[] parts)
        {
            Dictionary<string, string> Dic = new Dictionary<string, string>();

            try
            {
                for (int i = 0; i < parts.Length; i += 2)
                {
                    if (!Dic.ContainsKey(parts[i]))
                        Dic.Add(parts[i], parts[i + 1]);
                }
            }
            catch (IndexOutOfRangeException) { }

            return Dic;
        }
        public static string FixFilter(string filter)
        {
            // escape [
            filter = filter.Replace("[", "[[]");

            // fix an issue in the BF2 main menu where filter expressions aren't joined properly
            // i.e. "numplayers > 0gametype like '%gpm_cq%'"
            // becomes "numplayers > 0 && gametype like '%gpm_cq%'"
            try
            {
                filter = FixFilterOperators(filter);
            }
            catch (Exception e)
            {

            }

            // fix quotes inside quotes
            // i.e. hostname like 'flyin' high'
            // becomes hostname like 'flyin_ high'
            try
            {
                filter = FixFilterQuotes(filter);
            }
            catch (Exception e)
            {

            }

            // fix consecutive whitespace
            filter = Regex.Replace(filter, @"\s+", " ").Trim();

            return filter;
        }

        private static string FixFilterOperators(string filter)
        {
            PropertyInfo[] properties = typeof(PBF2Server).GetProperties();
            List<string> filterableProperties = new List<string>();

            // get all the properties that aren't "[NonFilter]"
            foreach (var property in properties)
            {
                if (property.GetCustomAttributes(false).Any(x => x.GetType().Name == "NonFilterAttribute"))
                    continue;

                filterableProperties.Add(property.Name);
            }

            // go through each property, see if they exist in the filter,
            // and check to see if what's before the property is a logical operator
            // if it is not, then we slap a && before it
            foreach (var property in filterableProperties)
            {
                IEnumerable<int> indexes = filter.IndexesOf(property);
                foreach (var index in indexes)
                {
                    if (index > 0)
                    {
                        int length = 0;
                        bool hasLogical = IsLogical(filter, index, out length, true) || IsOperator(filter, index, out length, true) || IsGroup(filter, index, out length, true);
                        if (!hasLogical)
                        {
                            filter = filter.Insert(index, " && ");
                        }
                    }
                }
            }
            return filter;
        }

        private static string FixFilterQuotes(string filter)
        {
            StringBuilder newFilter = new StringBuilder(filter);

            for (int i = 0; i < filter.Length; i++)
            {
                int length = 0;
                bool isOperator = IsOperator(filter, i, out length);

                if (isOperator)
                {
                    i += length;
                    bool isInsideString = false;
                    for (; i < filter.Length; i++)
                    {
                        if (filter[i] == '\'' || filter[i] == '"')
                        {
                            if (isInsideString)
                            {
                                // check what's after the quote to see if we terminate the string
                                if (i >= filter.Length - 1)
                                {
                                    // end of string
                                    isInsideString = false;
                                    break;
                                }
                                for (int j = i + 1; j < filter.Length; j++)
                                {
                                    // continue along whitespace
                                    if (filter[j] == ' ')
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        // if it's a logical operator, then we terminate
                                        bool op = IsLogical(filter, j, out length);
                                        if (op)
                                        {
                                            isInsideString = false;
                                            j += length;
                                            i = j;
                                        }
                                        break;
                                    }
                                }
                                if (isInsideString)
                                {
                                    // and if we're still inside the string, replace the quote with a wildcard character
                                    newFilter[i] = '_';
                                }
                                continue;
                            }
                            else
                            {
                                isInsideString = true;
                            }
                        }
                    }
                }
            }

            return newFilter.ToString();
        }
        private static bool IsOperator(string filter, int i, out int length, bool previous = false)
        {
            bool isOperator = false;
            length = 0;

            if (i < filter.Length - 1)
            {
                string op = filter.Substring(i - (i >= 2 ? (previous ? 2 : 0) : 0), 1);
                if (op == "=" || op == "<" || op == ">")
                {
                    isOperator = true;
                    length = 1;
                }
            }

            if (!isOperator)
            {
                if (i < filter.Length - 2)
                {
                    string op = filter.Substring(i - (i >= 3 ? (previous ? 3 : 0) : 0), 2);
                    if (op == "==" || op == "!=" || op == "<>" || op == "<=" || op == ">=")
                    {
                        isOperator = true;
                        length = 2;
                    }
                }
            }

            if (!isOperator)
            {
                if (i < filter.Length - 4)
                {
                    string op = filter.Substring(i - (i >= 5 ? (previous ? 5 : 0) : 0), 4);
                    if (op.Equals("like", StringComparison.InvariantCultureIgnoreCase))
                    {
                        isOperator = true;
                        length = 4;
                    }
                }
            }

            if (!isOperator)
            {
                if (i < filter.Length - 8)
                {
                    string op = filter.Substring(i - (i >= 9 ? (previous ? 9 : 0) : 0), 8);
                    if (op.Equals("not like", StringComparison.InvariantCultureIgnoreCase))
                    {
                        isOperator = true;
                        length = 8;
                    }
                }
            }

            return isOperator;
        }

        private static bool IsLogical(string filter, int i, out int length, bool previous = false)
        {
            bool isLogical = false;
            length = 0;

            if (i < filter.Length - 2)
            {
                string op = filter.Substring(i - (i >= 3 ? (previous ? 3 : 0) : 0), 2);
                if (op == "&&" || op == "||" || op.Equals("or", StringComparison.InvariantCultureIgnoreCase))
                {
                    isLogical = true;
                    length = 2;
                }
            }

            if (!isLogical)
            {
                if (i < filter.Length - 3)
                {
                    string op = filter.Substring(i - (i >= 4 ? (previous ? 4 : 0) : 0), 3);
                    if (op.Equals("and", StringComparison.InvariantCultureIgnoreCase))
                    {
                        isLogical = true;
                        length = 3;
                    }
                }
            }

            return isLogical;
        }

        private static bool IsGroup(string filter, int i, out int length, bool previous = false)
        {
            bool isGroup = false;
            length = 0;

            if (i < filter.Length - 1)
            {
                string op = filter.Substring(i - (i >= 2 ? (previous ? 2 : 0) : 0), 1);
                if (op == "(" || op == ")")
                {
                    isGroup = true;
                    length = 1;
                }
                if (!isGroup && previous)
                {
                    op = filter.Substring(i - (i >= 1 ? (previous ? 1 : 0) : 0), 1);
                    if (op == "(" || op == ")")
                    {
                        isGroup = true;
                        length = 1;
                    }
                }
            }

            return isGroup;
        }
        public static class GameSpyEncoding
        {
            public class GSEncodingData
            {
                public byte[] EncodingKey = new byte[261];

                public long Offset;

                public long Start;
            }

            public static byte[] Encode(byte[] key, byte[] validate, byte[] data, long size)
            {
                byte[] numArray = new byte[size + 23];
                byte[] numArray1 = new byte[23];
                if (key == null || validate == null || data == null || size < 0)
                {
                    return null;
                }
                int length = key.Length;
                int num = validate.Length;
                int num1 = (new Random()).Next();
                for (int i = 0; i < numArray1.Length; i++)
                {
                    num1 = num1 * 214013 + 2531011;
                    numArray1[i] = (byte)((num1 ^ key[i % length] ^ validate[i % num]) % 256);
                }
                numArray1[0] = 235;
                numArray1[1] = 0;
                numArray1[2] = 0;
                numArray1[8] = 228;
                for (long j = size - 1; j >= 0; j = j - 1)
                {
                    numArray[numArray1.Length + j] = data[j];
                }
                Array.Copy(numArray1, numArray, numArray1.Length);
                size = size + numArray1.Length;
                byte[] numArray2 = Two(key, validate, numArray, size, null);
                byte[] numArray3 = new byte[numArray2.Length + numArray1.Length];
                Array.Copy(numArray1, 0, numArray3, 0, numArray1.Length);
                Array.Copy(numArray2, 0, numArray3, numArray1.Length, numArray2.Length);
                return numArray3;
            }

            private static byte[] Two(byte[] u0002, byte[] u0003, byte[] u0005, long u0008, GSEncodingData u0006)
            {
                byte[] numArray;
                byte[] numArray1 = new byte[261];
                numArray = (u0006 != null ? u0006.EncodingKey : numArray1);
                if (u0006 == null || u0006.Start == 0)
                {
                    u0005 = Three(ref numArray, ref u0002, u0003, ref u0005, ref u0008, ref u0006);
                    if (u0005 == null)
                    {
                        return null;
                    }
                }
                if (u0006 == null)
                {
                    Four(ref numArray, ref u0005, u0008);
                    return u0005;
                }
                if (u0006.Start == 0)
                {
                    return null;
                }
                byte[] numArray2 = new byte[u0008 - u0006.Offset];
                Array.ConstrainedCopy(u0005, (int)u0006.Offset, numArray2, 0, (int)(u0008 - u0006.Offset));
                long num = Four(ref numArray, ref numArray2, u0008 - u0006.Offset);
                Array.ConstrainedCopy(numArray2, 0, u0005, (int)u0006.Offset, (int)(u0008 - u0006.Offset));
                GSEncodingData offset = u0006;
                offset.Offset = offset.Offset + num;
                byte[] numArray3 = new byte[u0008 - u0006.Start];
                Array.ConstrainedCopy(u0005, (int)u0006.Start, numArray3, 0, (int)(u0008 - u0006.Start));
                return numArray3;
            }

            private static byte[] Three(ref byte[] u0002, ref byte[] u0003, byte[] u0005, ref byte[] u0008, ref long u0006, ref GSEncodingData u000e)
            {
                long num = (long)((u0008[0] ^ 236) + 2);
                byte[] numArray = new byte[8];
                if (u0006 < (long)1)
                {
                    return null;
                }
                if (u0006 < num)
                {
                    return null;
                }
                long num1 = (long)(u0008[num - 1] ^ 234);
                if (u0006 < num + num1)
                {
                    return null;
                }
                Array.Copy(u0005, numArray, 8);
                byte[] numArray1 = new byte[u0006 - num];
                Array.ConstrainedCopy(u0008, (int)num, numArray1, 0, (int)(u0006 - num));
                Six(ref u0002, ref u0003, ref numArray, numArray1, num1);
                Array.ConstrainedCopy(numArray1, 0, u0008, (int)num, (int)(u0006 - num));
                num = num + num1;
                if (u000e != null)
                {
                    u000e.Offset = num;
                    u000e.Start = num;
                }
                else
                {
                    byte[] numArray2 = new byte[u0006 - num];
                    Array.ConstrainedCopy(u0008, (int)num, numArray2, 0, (int)(u0006 - num));
                    u0008 = numArray2;
                    u0006 = u0006 - num;
                }
                return u0008;
            }

            private static long Four(ref byte[] u0002, ref byte[] u0003, long u0005)
            {
                for (long i = 0; i < u0005; i = i + 1)
                {
                    u0003[i] = Five(ref u0002, u0003[i]);
                }
                return u0005;
            }

            private static byte Five(ref byte[] u0002, byte u0003)
            {
                int num = u0002[256];
                int num1 = u0002[257];
                int num2 = u0002[num];
                u0002[256] = (byte)((num + 1) % 256);
                u0002[257] = (byte)((num1 + num2) % 256);
                num = u0002[260];
                num1 = u0002[257];
                num1 = u0002[num1];
                num2 = u0002[num];
                u0002[num] = (byte)num1;
                num = u0002[259];
                num1 = u0002[257];
                num = u0002[num];
                u0002[num1] = (byte)num;
                num = u0002[256];
                num1 = u0002[259];
                num = u0002[num];
                u0002[num1] = (byte)num;
                num = u0002[256];
                u0002[num] = (byte)num2;
                num1 = u0002[258];
                num = u0002[num2];
                num2 = u0002[259];
                num1 = (num1 + num) % 256;
                u0002[258] = (byte)num1;
                num = num1;
                num2 = u0002[num2];
                num1 = u0002[257];
                num1 = u0002[num1];
                num = u0002[num];
                num2 = (num2 + num1) % 256;
                num1 = u0002[260];
                num1 = u0002[num1];
                num2 = (num2 + num1) % 256;
                num1 = u0002[num2];
                num2 = u0002[256];
                num2 = u0002[num2];
                num = (num + num2) % 256;
                num2 = u0002[num1];
                num1 = u0002[num];
                num2 = (num2 ^ num1 ^ u0003) % 256;
                u0002[260] = (byte)num2;
                u0002[259] = u0003;
                return (byte)num2;
            }

            // 78
            private static void Six(ref byte[] u0002, ref byte[] u0003, ref byte[] u0005, byte[] u0008, long u0006)
            {
                long length = (long)u0003.Length;
                for (long i = 0; i <= u0006 - 1; i = i + 1)
                {
                    u0005[u0003[i % length] * i & 7] = (byte)(u0005[u0003[i % length] * i & 7] ^ u0005[i & 7] ^ u0008[i]);
                }
                long num = 8;
                Seven(ref u0002, ref u0005, ref num);
            }

            // 89
            private static void Seven(ref byte[] u0002, ref byte[] u0003, ref long u0005)
            {
                long i;
                long num = 0;
                long num1 = 0;
                if (u0005 < 1)
                {
                    return;
                }
                for (i = 0; i <= 255; i = i + 1)
                {
                    u0002[i] = (byte)i;
                }
                for (i = 255; i >= 0; i = i - 1)
                {
                    byte num2 = (byte)Eight(u0002, i, u0003, u0005, ref num, ref num1);
                    byte num3 = u0002[i];
                    u0002[i] = u0002[num2];
                    u0002[num2] = num3;
                }
                u0002[256] = u0002[1];
                u0002[257] = u0002[3];
                u0002[258] = u0002[5];
                u0002[259] = u0002[7];
                u0002[260] = u0002[num & 255];
            }

            // 116
            private static long Eight(byte[] u0002, long u0003, byte[] u0005, long u0008, ref long u0006, ref long u000e)
            {
                long num;
                long num1 = (long)0;
                long num2 = (long)1;
                if (u0003 == (long)0)
                {
                    return 0;
                }
                if (u0003 > 1)
                {
                    do
                    {
                        num2 = (num2 << 1) + 1;
                    }
                    while (num2 < u0003);
                }
                do
                {
                    u0006 = (long)(u0002[u0006 & 255] + u0005[u000e]);
                    u000e = u000e + (long)1;
                    if (u000e >= u0008)
                    {
                        u000e = 0;
                        u0006 = u0006 + u0008;
                    }
                    num1 = num1 + 1;
                    num = (num1 <= 11 ? u0006 & num2 : u0006 & num2 % u0003);
                }
                while (num > u0003);
                return num;
            }
        }
    }
}