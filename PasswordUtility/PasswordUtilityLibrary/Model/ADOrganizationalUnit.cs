using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordUtilityLibrary
{
    public class ADOrganizationalUnit
    {
        public string DN { get; set; }
        public string Name { get; set; }

        public string DistinguishedName => this.DN;
        
        public List<ADOrganizationalUnit> Children;

        public ADOrganizationalUnit()
        {
            this.Children = new List<ADOrganizationalUnit>();
        }


        public void AddChild(ADOrganizationalUnit ou)
        {
            if (this.Children == null)
            {
                this.Children = new List<ADOrganizationalUnit>();
            }

            if (ou != null)
            {
                if (!this.Children.Contains(ou))
                {
                    this.Children.Add(ou);
                }
            }
        }

        public void AddChildren(List<ADOrganizationalUnit> ous)
        {
            if (this.Children == null)
            {
                this.Children = new List<ADOrganizationalUnit>();
            }

            if (ous?.Count > 0)
            {
                foreach (ADOrganizationalUnit ou in ous)
                {
                    this.AddChild(ou);
                }
            }
        }
    }
}
