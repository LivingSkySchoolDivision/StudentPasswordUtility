using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PasswordUtilityLibrary.Repositories
{
    public class ActiveDirectoryRepository
    {
        private readonly Config Config;

        public ActiveDirectoryRepository(Config config)
        {
            this.Config = config;
        }

        public List<ADUser> GetUsersInOU(string OuDn)
        {
            // This needs to only load user objects in an OU, and nothing else
            List<ADUser> returnedUsers = new List<ADUser>();
            DirectoryEntry searchRoot = Config.UseImpersonation ? new DirectoryEntry(Config.LDAPProtocol + OuDn, Config.ActiveDirectoryUsername, Config.ActiveDirectoryPassword) : new DirectoryEntry(Config.LDAPProtocol + OuDn);

            DirectorySearcher searcher = new DirectorySearcher(searchRoot)
            {
                Filter = "(objectClass=user)",
                Sort = new SortOption("sn", SortDirection.Ascending),
                PageSize = 1000
            };

            searcher.PropertiesToLoad.Add("givenName");
            searcher.PropertiesToLoad.Add("sn");
            searcher.PropertiesToLoad.Add("sAMAccountName");
            searcher.PropertiesToLoad.Add("comment");
            searcher.PropertiesToLoad.Add("employeeID");
            searcher.PropertiesToLoad.Add("userAccountControl");

            searcher.PropertiesToLoad.Add("badPwdCount");
            searcher.PropertiesToLoad.Add("mail");
            searcher.PropertiesToLoad.Add("badPasswordTime");
            searcher.PropertiesToLoad.Add("lastLogon");
            searcher.PropertiesToLoad.Add("lastLogonTimestamp");
            searcher.PropertiesToLoad.Add("pwdLastSet");

            // This scope returns all users in the given OU, and child OUs
            searcher.SearchScope = SearchScope.Subtree;

            // This scope returns just users in the given OU
            //searcher.SearchScope = SearchScope.OneLevel;

            SearchResultCollection allUsers = searcher.FindAll();

            //foreach (DirectoryEntry child in directoryObject.Children)
            foreach (SearchResult thisUser in allUsers)
            {
                DirectoryEntry child = thisUser.GetDirectoryEntry();
                string distinguishedName = child.Path.ToString().Remove(0, 7);
                string givenName = child.Properties.Contains("givenName") ? child.Properties["givenName"].Value.ToString() : "";
                string surname = child.Properties.Contains("sn") ? child.Properties["sn"].Value.ToString() : "";
                string sAMAccountName = child.Properties.Contains("sAMAccountName") ? child.Properties["sAMAccountName"].Value.ToString() : string.Empty;
                string comment = child.Properties.Contains("comment") ? child.Properties["comment"].Value.ToString() : "";
                string description = child.Properties.Contains("description") ? child.Properties["description"].Value.ToString() : "";
                string employeeID = child.Properties.Contains("employeeID") ? child.Properties["employeeID"].Value.ToString() : string.Empty;
                string employeeNumber = child.Properties.Contains("employeeNumber") ? child.Properties["employeeNumber"].Value.ToString() : string.Empty;
                int userAccountControl = Convert.ToInt32(child.Properties["userAccountControl"][0]);
                bool enabled = !((userAccountControl & 2) > 0);

                string mail = child.Properties.Contains("mail") ? child.Properties["mail"].Value.ToString() : "";
                int badPwdCount = child.Properties.Contains("badPwdCount") ? Parsers.ParseInt(child.Properties["badPwdCount"].Value.ToString()) : 0;
                
                DateTime pwdLastSet = DateTime.MinValue;
                if (child.Properties.Contains("pwdLastSet"))
                {
                    long value = (long)thisUser.Properties["pwdLastSet"][0];
                    pwdLastSet = DateTime.FromFileTime(value);
                }

                DateTime lastLogonTimestamp = DateTime.MinValue;
                if (child.Properties.Contains("lastLogonTimestamp"))
                {
                    long value = (long)thisUser.Properties["lastLogonTimestamp"][0];
                    lastLogonTimestamp = DateTime.FromFileTime(value);
                }

                DateTime lastLogon = DateTime.MinValue;
                if (child.Properties.Contains("lastLogon"))
                {
                    long value = (long)thisUser.Properties["lastLogon"][0];
                    lastLogon = DateTime.FromFileTime(value);
                }


                ADUser tempHumanUser = new ADUser()
                {
                    DN = distinguishedName,
                    GivenName = givenName,
                    Surname = surname,
                    sAMAccountName = sAMAccountName,
                    Comment = comment,
                    Description = description,
                    EmployeeNumber = employeeNumber,
                    EmployeeID = employeeID,
                    IsEnabled = enabled,
                    BadPasswordCount = badPwdCount,
                    Mail = mail,
                    PwdLastSet = pwdLastSet,
                    LastLogon = lastLogon,
                    LastLogonTimeStamp = lastLogonTimestamp
                };

                returnedUsers.Add(tempHumanUser);
                child.Close();
                child.Dispose();
            }
            searchRoot.Close();
            searchRoot.Dispose();

            return returnedUsers.OrderBy(u => u.ToString()).ToList();
        }

        public List<ADUser> GetUsersInOUs(List<string> distinguishedNames)
        {
            List<ADUser> returnedUsers = new List<ADUser>();
            foreach (string dn in distinguishedNames)
            {
                returnedUsers.AddRange(GetUsersInOU(dn));
            }
            return returnedUsers.OrderBy(u => u.ToString()).ToList();
        }
        
        public ADOrganizationalUnit GetOUTree(string OU)
        {
            DirectoryEntry rootDE = Config.UseImpersonation ? new DirectoryEntry(Config.LDAPProtocol + OU, Config.ActiveDirectoryUsername, Config.ActiveDirectoryPassword) : new DirectoryEntry(Config.LDAPProtocol + OU);
            return GetOUTree(rootDE);
        }

        public ADOrganizationalUnit GetOUTree(DirectoryEntry rootDE)
        {
            ADOrganizationalUnit rootOU = new ADOrganizationalUnit()
            {
                Name = rootDE.Properties["name"].Value.ToString(),
                DN = rootDE.Properties["distinguishedName"].Value.ToString()
            };

            // Create the root OU - ie: the one we've specified above
            // For each child, do the same, adding to this one's children list

            // Find all of this OU's child OUs
            DirectorySearcher ouSearch = new DirectorySearcher(rootDE, "(objectClass=organizationalUnit)", null, SearchScope.OneLevel);
            ouSearch.Sort = new SortOption("name", SortDirection.Ascending);

            foreach (SearchResult thisOU in ouSearch.FindAll())
            {
                DirectoryEntry thisOUDE = thisOU.GetDirectoryEntry();
                rootOU.AddChild(GetOUTree(thisOUDE));
            }

            rootDE.Dispose();
            return rootOU;
        }

        public void ExpirePassword(ADUser user)
        {
            DirectoryEntry directoryEntry = Config.UseImpersonation ? new DirectoryEntry(Config.LDAPProtocol + user.DN, Config.ActiveDirectoryUsername, Config.ActiveDirectoryPassword) : new DirectoryEntry(Config.LDAPProtocol + user.DN);

            directoryEntry.Properties["pwdLastSet"].Value = 0;

            directoryEntry.CommitChanges();
            directoryEntry.Close();
        }

        public void EnableUser(ADUser user)
        {
            DirectoryEntry directoryEntry = Config.UseImpersonation ? new DirectoryEntry(Config.LDAPProtocol + user.DN, Config.ActiveDirectoryUsername, Config.ActiveDirectoryPassword) : new DirectoryEntry(Config.LDAPProtocol + user.DN);

            int val = (int)directoryEntry.Properties["userAccountControl"].Value;
            directoryEntry.Properties["userAccountControl"].Value = val & ~0x2;

            directoryEntry.CommitChanges();
            directoryEntry.Close();

            user.IsEnabled = true;
        }

        public void DisableUser(ADUser user)
        {
            DirectoryEntry directoryEntry = Config.UseImpersonation ? new DirectoryEntry(Config.LDAPProtocol + user.DN, Config.ActiveDirectoryUsername, Config.ActiveDirectoryPassword) : new DirectoryEntry(Config.LDAPProtocol + user.DN);

            int val = (int)directoryEntry.Properties["userAccountControl"].Value;
            directoryEntry.Properties["userAccountControl"].Value = val | 0x2;

            directoryEntry.CommitChanges();
            directoryEntry.Close();

            user.IsEnabled = false;
        }

        public void SetComment(ADUser user, string newComment)
        {
            DirectoryEntry directoryEntry = Config.UseImpersonation ? new DirectoryEntry(Config.LDAPProtocol + user.DN, Config.ActiveDirectoryUsername, Config.ActiveDirectoryPassword) : new DirectoryEntry(Config.LDAPProtocol + user.DN);

            if (!string.IsNullOrEmpty(newComment))
            {
                directoryEntry.Properties["comment"].Value = newComment;
            }
            else
            {
                directoryEntry.Properties["comment"].Clear();
            }

            directoryEntry.CommitChanges();
            directoryEntry.Close();

            user.Comment = newComment;
        }

        public void SetDescription(ADUser user, string newDescription)
        {
            DirectoryEntry directoryEntry = Config.UseImpersonation ? new DirectoryEntry(Config.LDAPProtocol + user.DN, Config.ActiveDirectoryUsername, Config.ActiveDirectoryPassword) : new DirectoryEntry(Config.LDAPProtocol + user.DN);

            if (!string.IsNullOrEmpty(newDescription))
            {
                directoryEntry.Properties["description"].Value = newDescription;
            }
            else
            {
                directoryEntry.Properties["description"].Clear();
            }

            directoryEntry.CommitChanges();
            directoryEntry.Close();

            user.Description = newDescription;
        }

        public void ChangeUserPassword(ADUser user, string newpassword, bool? expirepassword)
        {
            string actualNewPassword = newpassword;
            if (string.IsNullOrEmpty(newpassword))
            {
                actualNewPassword = Config.DefaultNewPassword;
            }

            DirectoryEntry directoryEntry = Config.UseImpersonation ? new DirectoryEntry(Config.LDAPProtocol + user.DN, Config.ActiveDirectoryUsername, Config.ActiveDirectoryPassword) : new DirectoryEntry(Config.LDAPProtocol + user.DN);

            try
            {
                directoryEntry.Invoke("SetPassword", new object[] {actualNewPassword});
            }
            catch (TargetInvocationException ex)
            {
                // Because we're invoking something, the exception we get is mostly useless (exception thrown by target of invokation), so send the actual exception
                throw ex.InnerException;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (expirepassword == true)
            {
                directoryEntry.Properties["pwdLastSet"].Value = 0;
            }

            user.PwdLastSet = DateTime.Now;

            directoryEntry.CommitChanges();
            directoryEntry.Close();
        }
    }
}
