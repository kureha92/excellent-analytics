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
        ProxySettings _settings;
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
            Authenticate();
        }

        internal void Authenticate()
        {
            if (this.activeUser == null)
            {
                string email = textBEmail.Text;
                string password = textBPassword.Password;
                AccountManager accMan = new AccountManager();
                accMan.authProgress += new AccountManager.AuthProgress(accMan_authProgress);

                HttpStatusCode status;
                string authToken = accMan.Authenticate(email, password, out status);
                if (!String.IsNullOrEmpty(authToken) && status == HttpStatusCode.OK && authSuccessful != null)
                {
                    this.Close();
                    authSuccessful(authToken, email);
                }
                else if (status == HttpStatusCode.ProxyAuthenticationRequired) {
                    this._settings = new ProxySettings();
                    this._settings._loginReferenceForm = this;
                    _settings.ShowDialog();
                }
                else {
                    mainNotify.Visibility = Visibility.Visible;
                    mainNotify.ErrorMessage = status == HttpStatusCode.Forbidden ? "Invalid password / username" :
                    status == HttpStatusCode.NotFound ? "A connection could not be established" : "Error: " + status.ToString();
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
             mainNotify.ErrorMessage = progressMessage;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Canvas_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                VisualStateManager.GoToState(buttonLogin, "Pressed", true);
                Authenticate();
            }
        }
    }
}
