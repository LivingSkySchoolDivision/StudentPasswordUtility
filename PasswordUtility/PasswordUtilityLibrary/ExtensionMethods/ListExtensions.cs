using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordUtilityLibrary.ExtensionMethods
{
    public static class ListExtensions
    {
        public static string ToSemicolenSeparatedString(this List<string> list)
        {
            StringBuilder returnMe = new StringBuilder();

            foreach (string item in list)
            {
                returnMe.Append(item);
                returnMe.Append(";");
            }

            if (returnMe.Length > 1)
            {
                returnMe.Remove(returnMe.Length - 1, 1);
            }

            return returnMe.ToString();
        }
    }
}
