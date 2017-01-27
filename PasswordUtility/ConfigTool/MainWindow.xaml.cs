using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PasswordUtilityLibrary;
using PasswordUtilityLibrary.ExtensionMethods;
using PasswordUtilityLibrary.Repositories;

namespace ConfigTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Config Config;
        private static ActiveDirectoryRepository activeDirectoryRepository;
        
        public MainWindow()
        {
            InitializeComponent();
        }
        
        private TreeViewItem convertToTreeViewItem(ADOrganizationalUnit ou)
        {
            TreeViewItem returnMe = new TreeViewItem() { Header = ou.Name, Tag = ou };

            if (ou.Children.Count > 0)
            {
                foreach (ADOrganizationalUnit child in ou.Children)
                {
                    returnMe.Items.Add(convertToTreeViewItem(child));
                }
            }
            return returnMe;
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Config = new Config();
            activeDirectoryRepository = new ActiveDirectoryRepository(Config);
            
            txtUsername.Text = Config.ActiveDirectoryUsername;
            chkImpersonate.IsChecked = Config.UseImpersonation;
            chkUseLDAPS.IsChecked = Config.UseLDAPS;

            // Get the root directoryentry for the domain that this program is running on, so we can show the entire OU tree
            DirectoryEntry rootDSE = new DirectoryEntry("LDAP://RootDSE");
            string rootOU_DN = rootDSE.Properties["defaultNamingContext"].Value.ToString();

            txtOUDN.Text = Config.ActiveDirectoryRootOUs.ToSemicolenSeparatedString();

            try
            {
                ADOrganizationalUnit allOUTree = activeDirectoryRepository.GetOUTree(rootOU_DN);
                TreeViewItem tree = convertToTreeViewItem(allOUTree);
                treeOUList.Items.Add(tree);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading from AD: " + ex.Message, "Error loading from AD", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void btnSaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Check to see if we need to modify the username and password
            Config.ActiveDirectoryUsername = txtUsername.Text;

            if (!string.IsNullOrEmpty(this.pwdPassword.Password))
            {
                Config.ActiveDirectoryPassword = pwdPassword.Password;
            }

            if (!string.IsNullOrEmpty(txtDefaultPassword.Text.Trim()))
            {
                Config.DefaultNewPassword = txtDefaultPassword.Text.Trim();
            }

            // This utility doesn't yet support multiple OUs, but you can manually enter them in the config file. 
            Config.ActiveDirectoryRootOUs = new List<string>() { txtOUDN.Text.Trim() };
            Config.UseImpersonation = chkImpersonate.IsChecked == true;
            Config.UseLDAPS = chkUseLDAPS.IsChecked == true;
            Config.Commit();
            Close();
        }

        private void btnUpdateOU_Click(object sender, RoutedEventArgs e)
        {
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void treeOUList_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeView thisTreeView = (TreeView)sender;

            try
            {
                TreeViewItem selectedItem = (TreeViewItem)thisTreeView.SelectedItem;
                ADOrganizationalUnit selectedOU = (ADOrganizationalUnit)selectedItem.Tag;
                txtOUDN.Text = selectedOU.DN;
            }
            catch { }
        }

        private void chkImpersonate_Checked(object sender, RoutedEventArgs e)
        {
            if (chkImpersonate.IsChecked == true)
            {
                txtUsername.IsEnabled = true;
                pwdPassword.IsEnabled = true;
            }
            else
            {
                txtUsername.IsEnabled = false;
                pwdPassword.IsEnabled = false;
            }
        }
    }
}
