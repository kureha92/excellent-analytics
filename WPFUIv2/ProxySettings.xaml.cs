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
using Analytics;

namespace UI {
    /// <summary>
    /// Interaction logic for ProxySettings.xaml
    /// </summary>
    public partial class ProxySettings : Window {
        public Login _loginReferenceForm;
        public ProxySettings() {
            InitializeComponent();
        }


        private void Button_Click(object sender, RoutedEventArgs e) {
            if (!string.IsNullOrEmpty(txtProxyUserID.Text) && !string.IsNullOrEmpty(txtProxyPassword.Password) && !string.IsNullOrEmpty(txtProxyAddress.Text) && !string.IsNullOrEmpty(txtProxyPort.Text)) {
                ProxyHelper._proxyAddress = txtProxyAddress.Text;
                ProxyHelper._proxyPort = Convert.ToInt32(txtProxyPort.Text);
                ProxyHelper._isProxyAuthSet = true;
                ProxyHelper._proxyUserName = txtProxyUserID.Text;
                ProxyHelper._proxyUserPassword = txtProxyPassword.Password;
                this.Close();
                this._loginReferenceForm.Authenticate();
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void Canvas_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                VisualStateManager.GoToState(buttonPLogin, "Pressed", true);
            }
        }
    }
}
