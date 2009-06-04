using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Analytics.Authorization;
using System.Net;
using System.Windows.Threading;
using System.Threading;

namespace UI
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public delegate void AuthSuccessful(string authToken, string email);
        public event AuthSuccessful authSuccessful;

        public delegate void Logout();
        public event Logout logOut;

        UserAccount activeUser;

        public Login(UserAccount uAcc)
        {
            InitializeComponent();

            if (uAcc != null && !String.IsNullOrEmpty(uAcc.AuthToken))
            {
                this.activeUser = uAcc;
                buttonLogin.Content = "Log out";
                textBEmail.Text = this.activeUser.EMail;
                textBEmail.IsEnabled = false;
                textBPassword.IsEnabled = false;
            }
            else
            {
                textBEmail.Focus();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.activeUser == null)
            {
                progressLabel.Visibility = Visibility.Visible;

                string email = textBEmail.Text;
                string password = textBPassword.Password;
                AccountManager accMan = new AccountManager();
                accMan.authProgress += new AccountManager.AuthProgress(accMan_authProgress);

                HttpStatusCode status;
                string authToken = accMan.Authenticate(email, password, out status);
                if (!String.IsNullOrEmpty(authToken) && authSuccessful != null)
                {
                    this.Close();
                    authSuccessful(authToken, email);
                }
            }
            else
            {
                buttonLogin.Content = "Log in";
                textBEmail.Text = string.Empty;
                textBEmail.IsEnabled = true;
                textBPassword.IsEnabled = true;
                activeUser = null;
                if (logOut != null)
                {
                    logOut();
                }
            }
        }

        void accMan_authProgress(int progress, string progressMessage)
        {
            progressLabel.Content = progressMessage;
        }

        private void textBPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {

        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
