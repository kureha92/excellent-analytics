
using System;
using System.Windows;
using System.Windows.Input;
using Analytics.Authorization;
using System.Net;
using System.Security.Cryptography;

namespace UI
{
	/// <summary>
	/// Interaction logic for Login.xaml
	/// </summary>
	public partial class Login : Window
	{
		public delegate void AuthSuccessful(string authToken, string email);
		public event AuthSuccessful authSuccessful;
		//ProxySettings _settings;
		public delegate void Logout();
		public event Logout logOut;
		UserAccount activeUser;
		string userName;
		string password;

		private byte[] randomBytes = { 4,32,62,9,145,5};

		public Login(UserAccount uAcc)
		{
			InitializeComponent();

			/*if (userName != null)
				textBEmail.Text = userName;

			if (password != null)
			{
				textBPassword.Password = password;
				passwordRemember.IsChecked = true;
			}*/

			if (textBPassword.Padding != null)
				passwordRemember.Visibility = Visibility.Visible;

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
                    DialogResult = true;
					this.Close();
					authSuccessful(authToken, email);
				}
				else if (status == HttpStatusCode.ProxyAuthenticationRequired)
				{
                    MessageBox.Show("Authentication request failed. It seems that you are behind a proxy. Set the proxy in the settings dialog and try again.", "Authentation request failed", MessageBoxButton.OK, MessageBoxImage.Warning);
					//this._settings = new ProxySettings();
					//this._settings._loginReferenceForm = this;
					//_settings.ShowDialog();
				}
				else
				{
					mainNotify.Visibility = Visibility.Visible;
					mainNotify.ErrorMessage = status == HttpStatusCode.Forbidden ? "Invalid password / username" :
					status == HttpStatusCode.NotFound ? "A connection could not be established" : "Error: " + status.ToString();
				}
			}
			else
			{
				buttonLogin.Content = "Log in";
				textBEmail.Clear();
				textBPassword.Clear();
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
            DialogResult = false;
			this.Close();
		}

		private void Canvas_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				System.Windows.VisualStateManager.GoToState(buttonLogin, "Pressed", true);
				Authenticate();
			}
		}

        public string Username
        {
            get { return textBEmail.Text; }
            set { textBEmail.Text = value; }
        }

        public string Password
        {
            get { return textBPassword.Password; }
            set { textBPassword.Password = value;  }
        }

        public bool RememberPassword
        {
            get { return passwordRemember.IsChecked.GetValueOrDefault(false);  }
            set { passwordRemember.IsChecked = value;}
        }

	}
}
