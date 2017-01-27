using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordUtilityLibrary
{
    public class ADUser
    {
        public string sAMAccountName { get; set; }
        public string GivenName { get; set; }
        public string Surname { get; set; }
        public string DN { get; set; }
        public string Comment { get; set; }
        public string Description { get; set; }
        public string EmployeeID { get; set; }
        public string EmployeeNumber { get; set; }
        public string Mail { get; set; }
        public DateTime LastLogon { get; set; }
        public DateTime LastLogonTimeStamp { get; set; }
        public int BadPasswordCount { get; set; }
        public DateTime PwdLastSet { get; set; }
        public bool IsEnabled { get; set; }

        // I can't seem to get this to return a value from AD
        //public DateTime BadPasswordTime { get; set; }


        public string FirstName => this.GivenName;
        public string LastName => this.Surname;
        public string DistinguishedName => this.DN;


        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.EmployeeID))
            {
                return this.Surname + ", " + this.GivenName + " (" + this.EmployeeID + ")";
            }

            if (!string.IsNullOrEmpty(this.EmployeeNumber))
            {
                return this.Surname + ", " + this.GivenName + " ( " + this.EmployeeNumber + ")";
            }

            return this.Surname + ", " + this.GivenName;
        }
    }
}
