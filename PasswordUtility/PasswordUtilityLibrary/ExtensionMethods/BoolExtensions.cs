using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordUtilityLibrary.ExtensionMethods
{
    public static class BoolExtensions
    {
        public static string ToYesOrNo(this bool yn)
        {
            return yn ? "Yes" : "No";
        }
    }
}
