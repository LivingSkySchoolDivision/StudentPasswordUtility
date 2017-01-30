using System;
using System.Collections.Generic;
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
using System.Windows.Threading;
using PasswordUtilityLibrary;
using PasswordUtilityLibrary.Repositories;

namespace PasswordUtility
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Config Config;
        private static ActiveDirectoryRepository adRepo;
        private static List<ADUser> ADUsers = new List<ADUser>();
        private static ADUser selectedUser;

        private void blankFields()
        {
            txtIDNumber.Text = "";
            txtFirstName.Text = "";
            txtLastName.Text = "";
            txtUserName.Text = "";
            txtComment.Text = "";
            txtSecurity.Text = "";
            txtPwdLastSet.Text = "";
            txtLastLogon.Text = "";

            btnChangePassword.IsEnabled = false;
            btnDisable.IsEnabled = false;
            btnEnable.IsEnabled = false;
            btnUpdateDescription.IsEnabled = false;
            chkPromptToChange.IsEnabled = false;
        }

        private void displayUser(ADUser user)
        {
            txtIDNumber.Text = user.EmployeeID;
            txtFirstName.Text = user.FirstName;
            txtLastName.Text = user.LastName;
            txtUserName.Text = user.sAMAccountName;
            txtComment.Text = user.Comment;
            txtPwdLastSet.Text = user.PwdLastSet.ToString();
            txtLastLogon.Text = user.LastLogonTimeStamp.ToString();
            btnChangePassword.IsEnabled = true;
            btnUpdateDescription.IsEnabled = true;
            chkPromptToChange.IsEnabled = true;
            txtPassword.IsEnabled = true;

            updateEnableDisableButtons();
        }

        private void UpdateAccountListBox()
        {
            try
            {
                lstAccountNames.Items.Clear();
                foreach (ADUser user in ADUsers)
                {
                    ListViewItem newListItem = new ListViewItem
                    {
                        Content = user
                    };

                    if (
                        (txtSearchBox.Text.ToLower().Equals("search...")) ||
                        (string.IsNullOrEmpty(txtSearchBox.Text)) ||
                        (user.sAMAccountName.ToLower().Contains(txtSearchBox.Text.ToLower())) ||
                        (user.FirstName.ToLower().Contains(txtSearchBox.Text.ToLower())) ||
                        (user.LastName.ToLower().Contains(txtSearchBox.Text.ToLower())) ||
                        (user.EmployeeID.ToLower().Contains(txtSearchBox.Text.ToLower()))
                        )
                    {
                        lstAccountNames.Items.Add(newListItem);
                    }
                }



            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error displaying users", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        

        private delegate void UpdateAccountListDelegate();
        private void RefreshADUserListFromAD()
        {
            try
            {
                // Reset the selected user
                selectedUser = null;

                lstAccountNames.Items.Clear();
                lstAccountNames.Items.Add("Loading...");

                lstAccountNames.IsEnabled = false;
                txtSearchBox.IsEnabled = false;
                txtComment.IsEnabled = false;
                btnRefreshAccountsList.IsEnabled = false;
                btnRefreshAccountsList.Content = "Loading...";
                progressBar.Visibility = Visibility.Visible;
                lblStudentTotal.Visibility = Visibility.Hidden;
                blankFields();

                Task.Factory.StartNew(() =>
                        {
                            if (Config.ActiveDirectoryRootOUs.Count > 0)
                            {
                                ADUsers = adRepo.GetUsersInOUs(Config.ActiveDirectoryRootOUs);
                            }

                            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (UpdateAccountListDelegate) delegate()
                            {
                                // Update list
                                UpdateAccountListBox();

                                // Update UI to reflect that data was loaded
                                progressBar.Visibility = Visibility.Hidden;
                                btnRefreshAccountsList.IsEnabled = true;
                                btnRefreshAccountsList.Content = "Refresh user list";
                                lstAccountNames.IsEnabled = true;
                                txtSearchBox.IsEnabled = true;
                                txtComment.IsEnabled = true;
                                lblStudentTotal.Visibility = Visibility.Visible;
                                lblStudentTotal.Content = "Total accounts listed: " + ADUsers.Count;
                            });
                        }
                    );
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error loading users", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            ADUsers = new List<ADUser>();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Config = new Config();
                
            if (Config.ActiveDirectoryRootOUs.Count <= 0)
            {
                MessageBox.Show("ERROR: No OUs are configured, so no users can be loaded. Please have your IT department create a configuration file.", "Error loading config file", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }

            adRepo = new ActiveDirectoryRepository(Config);
            lblBlankPassword.Content = "If left blank, password will be: " + Config.DefaultNewPassword;

            // Load AD users
            RefreshADUserListFromAD();
        }

        private void updateEnableDisableButtons()
        {
            if (selectedUser != null)
            {
                if (selectedUser.IsEnabled)
                {
                    txtSecurity.Text = "Account is Enabled";
                    txtSecurity.Foreground = Brushes.DarkGreen;
                    txtSecurity.FontWeight = FontWeights.Normal;
                    btnEnable.IsEnabled = false;
                    btnDisable.IsEnabled = true;
                }
                else
                {
                    txtSecurity.Text = "Account is Disabled";
                    txtSecurity.Foreground = Brushes.Red;
                    txtSecurity.FontWeight = FontWeights.Bold;
                    btnEnable.IsEnabled = true;
                    btnDisable.IsEnabled = false;
                }
            }
        }


        private void listAccountNames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox thisListBox = (ListBox) sender;
            try
            {
                if (thisListBox.SelectedItem != null)
                {
                    if (thisListBox.SelectedItem.GetType() == typeof(ADUser))
                    {
                        selectedUser = (ADUser) thisListBox.SelectedItem;
                        displayUser(selectedUser);

                    }
                    else if (thisListBox.SelectedItem.GetType() == typeof(ListViewItem))
                    {

                        ListViewItem lvi = (ListViewItem)thisListBox.SelectedItem;
                        selectedUser = (ADUser) lvi.Content;
                        displayUser(selectedUser);
                    }
                    else if (thisListBox.SelectedItem.GetType() == typeof(ListBoxItem))
                    {
                        ListBoxItem lvi = (ListBoxItem)thisListBox.SelectedItem;
                        selectedUser = (ADUser) lvi.Content;
                        displayUser(selectedUser);
                    }
                }
            }
            catch { } // Ignore UI errors that can occur if the listbox is disabled or not loaded yet

        }
        
        private void btnUpdateDescription_Click(object sender, RoutedEventArgs e)
        {
            if (selectedUser != null)
            {
                try
                {
                    adRepo.SetComment(selectedUser, txtComment.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error updating user: " + ex.Message, "Error updating user", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            if (selectedUser != null)
            {
                try
                {
                    adRepo.ChangeUserPassword(selectedUser, txtPassword.Text.Trim(), chkPromptToChange.IsChecked);
                    MessageBox.Show("Password for " + selectedUser.sAMAccountName + " changed!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error updating user: " + ex.Message, "Error updating user", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnEnable_Click(object sender, RoutedEventArgs e)
        {
            if (selectedUser != null)
            {
                try
                {
                    adRepo.EnableUser(selectedUser);
                    updateEnableDisableButtons();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error updating user: " + ex.Message, "Error updating user", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnDisable_Click(object sender, RoutedEventArgs e)
        {
            if (selectedUser != null)
            {
                try
                {
                    adRepo.DisableUser(selectedUser);
                    updateEnableDisableButtons();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error updating user: " + ex.Message, "Error updating user", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnRefreshAccountsList_Click(object sender, RoutedEventArgs e)
        {
            RefreshADUserListFromAD();
        }

        private void txtSearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtSearchBox.Text == "Search...")
            {
                txtSearchBox.Text = "";
            }
        }

        private void txtSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateAccountListBox();
        }
    }
}
