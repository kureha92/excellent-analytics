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
    /// Interaction logic for UpdateDialog.xaml
    /// </summary>
    public partial class UpdateDialog : Window
    {
        private Dictionary<DateTime, string> remindMeAgainPeriods = new Dictionary<DateTime, string>();
        public UpdateDialog()
        {
            InitializeComponent();

            remindMeAgainPeriods.Add(DateTime.Now.AddDays(1), "In a day");
            remindMeAgainPeriods.Add(DateTime.Now.AddDays(7), "In a week");
            remindMeAgainPeriods.Add(DateTime.Now.AddMonths(1), "In a Month");

            Binding remindMeBinding = new Binding();
            remindMeBinding.Source = remindMeAgainPeriods;
            RemindMeAgainDDL.DisplayMemberPath = "Value";
            RemindMeAgainDDL.SelectedValuePath = "Key";
            RemindMeAgainDDL.SetBinding(ComboBox.ItemsSourceProperty, remindMeBinding);
            RemindMeAgainDDL.SelectedIndex = 0;
        }

        public DateTime GetRemindMeAgainDate()
        {
            return (RemindMeAgainDDL.SelectedValue as DateTime?) ?? DateTime.Now.AddDays(-1);
        }

        private void UpdateNowButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
