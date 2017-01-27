using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using PasswordUtilityLibrary.ExtensionMethods;

namespace PasswordUtilityLibrary
{
    public class Config
    {
        private static readonly string configFileName = "config.xml";

        public string ActiveDirectoryUsername { get; set; }

        public string ActiveDirectoryPassword { get; set; }

        public List<string> ActiveDirectoryRootOUs { get; set; }

        public string DefaultNewPassword { get; set; }

        public bool UseLDAPS { get; set; }
        public bool UseImpersonation { get; set; }



        public string LDAPProtocol
        {
            get {
                return this.UseLDAPS ? "LDAPS://" : "LDAP://";
            }
        }


        private bool DoesConfigFileExist()
        {
            if (File.Exists(configFileName))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private string valueOrEmptyString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }
            else
            {
                return value;
            }
        }

        public Config()
        {
            this.ActiveDirectoryRootOUs = new List<string>();
            try
            {
                XElement configFileXML = XElement.Load(configFileName);
                
                foreach (XElement setting in configFileXML.Elements())
                {
                    if (setting.Name == "ActiveDirectoryUsername") { this.ActiveDirectoryUsername = Crypto.DecryptStringAES(valueOrEmptyString(setting.Value)); }
                    if (setting.Name == "ActiveDirectoryPassword") { this.ActiveDirectoryPassword = Crypto.DecryptStringAES(valueOrEmptyString(setting.Value)); }
                    if (setting.Name == "UseImpersonation") { this.UseImpersonation = (setting.Value.ToLower() == "yes"); }
                    if (setting.Name == "UseLDAPS") { this.UseLDAPS = (setting.Value.ToLower() == "yes"); }
                    if (setting.Name == "DefaultNewPassword") { this.DefaultNewPassword = setting.Value; }
                    if (setting.Name == "OU") { this.ActiveDirectoryRootOUs = valueOrEmptyString(setting.Value).Split(';').ToList(); }
                }
            }
            catch { }
        }

        public void Commit()
        {
            string[] configFileLines =
            {
                "<?xml version=\"1.0\" encoding=\"utf-8\" ?>",
                "<Settings>",
                "  <ActiveDirectoryUsername>" + Crypto.EncryptStringAES(this.ActiveDirectoryUsername) + "</ActiveDirectoryUsername>",
                "  <ActiveDirectoryPassword>" + Crypto.EncryptStringAES(this.ActiveDirectoryPassword) + "</ActiveDirectoryPassword>",
                "  <UseImpersonation>" + this.UseImpersonation.ToYesOrNo() + "</UseImpersonation>",
                "  <UseLDAPS>" + this.UseLDAPS.ToYesOrNo() + "</UseLDAPS>",
                "  <DefaultNewPassword>" + this.DefaultNewPassword + "</DefaultNewPassword>",
                "  <OU>" + this.ActiveDirectoryRootOUs.ToSemicolenSeparatedString() + "</OU>",
                "</Settings>"
            };

            System.IO.File.WriteAllLines(configFileName, configFileLines);
        }

    }
}
