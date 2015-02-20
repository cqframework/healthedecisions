using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HeD.Engine.Verification
{
    public static class StringExtensions
    {
        public static bool IsDigit(this string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                if (!Char.IsDigit(s, i))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
