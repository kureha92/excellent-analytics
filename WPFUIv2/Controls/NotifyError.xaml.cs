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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UI.Controls
{
    public partial class NotifyError : UserControl
    {
        static readonly DependencyProperty Message;

        public  string ErrorMessage
        {
            get { return (string)this.GetValue(Message); }
            set 
            {
                this.MessageLabel.Text = value;
                this.SetValue(Message, value); 
            }
        } 

        static NotifyError()
        {
            Message = DependencyProperty.Register("Message",
            typeof(string), typeof(NotifyError), new FrameworkPropertyMetadata(OnMessageChanged));
        }

        private static void OnMessageChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            dependencyObject.SetValue(NotifyError.Message, (string)e.NewValue);
        }

        public NotifyError()
        {
            InitializeComponent();
        }
    }
}
