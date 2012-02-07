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

namespace WPFUIv2
{

    /// <summary>
    /// Interaction logic for SettingsDialog.xaml
    /// </summary>
    public partial class SettingsDialog : Window
    {
        private bool changes = false;

        public SettingsDialog()
        {
            InitializeComponent();

            UseProxyCheckBox.Checked += new RoutedEventHandler(ChangesMade);
            UseProxyCheckBox.Unchecked += new RoutedEventHandler(ChangesMade);
            UsernameBox.TextChanged += new TextChangedEventHandler(ChangesMade);
            PasswordBox.PasswordChanged += new RoutedEventHandler(ChangesMade);
            PortBox.TextChanged += new TextChangedEventHandler(ChangesMade);
            AddressBox.TextChanged += new TextChangedEventHandler(ChangesMade);
            RequestTimeoutBox.TextChanged += new TextChangedEventHandler(ChangesMade);

            CellFormattingBox.Items.Add(new CellFormatting("Always ask", CellFormattingEnum.ask));
            CellFormattingBox.Items.Add(new CellFormatting("Always save", CellFormattingEnum.save));
            CellFormattingBox.Items.Add(new CellFormatting("Never save", CellFormattingEnum.never));
            CellFormattingBox.SelectionChanged += new SelectionChangedEventHandler(ChangesMade);
            
            this.Closing += new System.ComponentModel.CancelEventHandler(SettingsDialog_Closing);
        }
        public new bool? ShowDialog()
        {
            changes = false;
            return base.ShowDialog();
        }
        void SettingsDialog_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel =
                changes && MessageBox.Show(
                                "You've made changes to the settings. Are you sure you want to discard these changes?",
                                "Discard changes?",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning) == MessageBoxResult.No;
            
        }
        protected void ChangesMade(object sender, RoutedEventArgs args)
        {
            changes = true;
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            DialogResult = true;
            Close();
        }
        private void SaveSettings()
        {
            uint port;
            if (!uint.TryParse(PortBox.Text, out port))
            {
                MessageBox.Show("Entered port number is not a valid port.", "Invalid port", MessageBoxButton.OK, MessageBoxImage.Error);
                PortBox.Focus();
                return;
            }
            changes = false;
        }
        private void UpdateMetricsButton_Click(object sender, RoutedEventArgs e)
        {

            Microsoft.Win32.OpenFileDialog dlg = GetXmlDialog("metrics.xml");
            // Show open file dialog box
            bool? result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                ChangesMade(this, new RoutedEventArgs());
                MetricsFileName = dlg.FileName;
                UpdateMetricsButton.Content = MetricsFileName.Substring(MetricsFileName.LastIndexOf('\\')+1);
            }
        }
        private Microsoft.Win32.OpenFileDialog GetXmlDialog(string filename)
        {
            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = filename; // Default file name
            dlg.DefaultExt = ".xml"; // Default file extension
            dlg.Filter = "Xml document (.xml)|*.xml"; // Filter files by extension
            return dlg;
        }
        private void UpdateDimensionsButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = GetXmlDialog("dimensions.xml");

            // Show open file dialog box
            bool? result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                ChangesMade(this, new RoutedEventArgs());
                DimensionsFileName = dlg.FileName;
                UpdateDimensionsButton.Content = DimensionsFileName.Substring(DimensionsFileName.LastIndexOf('\\') + 1);
            }
        }

        public string DimensionsFileName { get; private set; }
        public string MetricsFileName { get; private set; }
        public bool UseProxy { 
            get { return UseProxyCheckBox.IsChecked.GetValueOrDefault(false); }
            set { UseProxyCheckBox.IsChecked = value;  }
        }
        public string ProxyAddress
        {
            get { return AddressBox.Text;  }
            set { AddressBox.Text = value;  }
        }
        public uint ProxyPort 
        {
            get { uint port; return uint.TryParse(PortBox.Text, out port) ? port : 0;  }
            set { PortBox.Text = value.ToString();  }
        }
        public string ProxyUsername
        {
            get { return UsernameBox.Text; }
            set { UsernameBox.Text = value; }
        }
        public string ProxyPassword
        {
            get { return PasswordBox.Password; }
            set { PasswordBox.Password = value; }
        }
        public uint RequestTimeout
        {
            get { uint timeout; return uint.TryParse(RequestTimeoutBox.Text, out timeout) ? timeout : 0; }
            set { RequestTimeoutBox.Text = value.ToString(); }
        }
        public CellFormattingEnum CellFormatting { 
            get { return ((CellFormatting)CellFormattingBox.SelectedValue).Value; }
            set {
                int i = 0;
                foreach (var item in CellFormattingBox.Items)
                {
                    if (((CellFormatting)item).Value == value)
                    {
                        CellFormattingBox.SelectedIndex = i;
                        break;
                    }
                    ++i;
                }
            }
        }
        
    }

    public enum CellFormattingEnum
    {
        save = 0,
        ask = 1,
        never = 2
    }

    public class CellFormatting
    {
        private string text;
        private CellFormattingEnum value;
        public CellFormatting(string text, CellFormattingEnum value)
        {
            this.text = text;
            this.value = value;
        }

        public CellFormattingEnum Value { get { return value; } }

        public override bool Equals(object obj)
        {
            return ((CellFormatting)obj).Value == Value;
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override string ToString()
        {
            return this.text;
        }
    }
}
