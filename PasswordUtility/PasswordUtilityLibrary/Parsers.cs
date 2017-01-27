using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordUtilityLibrary
{
    public static class Parsers
    {
        public static int ParseInt(string thisString)
        {
            int returnMe = 0;

            if (Int32.TryParse(thisString, out returnMe))
            {
                return returnMe;
            }

            return 0;
        }


        public static long ParseLongInt(string thisString)
        {
            long returnMe = 0;

            if (long.TryParse(thisString, out returnMe))
            {
                return returnMe;
            }

            return 0;
        }
    }
}
