using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B2BF.Common.Extensions
{
    public static class StringArrayExtensions
    {
        public static string GetParameterValue(this string[] parts, string parameter)
        {
            bool next = false;
            foreach (var part in parts)
            {
                if (next) return part;
                next = part == parameter;
            }

            return "";
        }
    }
}