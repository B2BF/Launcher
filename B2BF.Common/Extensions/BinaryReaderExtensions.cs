using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B2BF.Common.Extensions
{
    public static class BinaryReaderExtensions
    {
        public static string ReadNullTerminatedString(this BinaryReader reader)
        {
            byte b;
            var val = "";
            while (reader.BaseStream.Position < reader.BaseStream.Length && (b = reader.ReadByte()) != 0x00)
            {
                val += (char)b;
            }

            return val;
        }
    }
}