using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B2BF.Common.Extensions
{
    public static class StringExtensions
    {
        public static IEnumerable<int> IndexesOf(this string str, string str2)
        {
            int lastIndex = 0;
            while (true)
            {
                int index = str.IndexOf(str2, lastIndex);
                if (index == -1)
                {
                    yield break;
                }
                yield return index;
                lastIndex = index + str2.Length;
            }
        }

        public static byte[] FromHex(this string hex)
        {
            hex = hex.Replace("-", "");
            byte[] raw = new byte[hex.Length / 2];
            for (int i = 0; i < raw.Length; i++)
            {
                raw[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return raw;
        }
    }
}