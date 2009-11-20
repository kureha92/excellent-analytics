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
    /// <summary>
    /// Interaction logic for CustomTreeView.xaml
    /// </summary>
    public partial class CustomTreeView : UserControl
    {
        public enum DataType {Dimension, Metric}

        static readonly DependencyProperty Type;  

        public DataType ItemsDataType
        {
            get
            {
                return (DataType)this.GetValue(Type);
            }
            set
            {
                this.SetValue(Type, value);
            }
        }

        static CustomTreeView()
        {
            Type = DependencyProperty.Register("Type",
            typeof(DataType), typeof(CustomTreeView), new FrameworkPropertyMetadata(OnTypeChanged));
        }

        public CustomTreeView()
        {
            InitializeComponent();
        }

        private static void OnTypeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            dependencyObject.SetValue(CustomTreeView.Type , (DataType)e.NewValue);
        }
       
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = ItemsDataType == DataType.Dimension ?
            Resources["dimProvider"] : Resources["metProvider"];
        }

        private void UncheckAll_Click(object sender, RoutedEventArgs e)
        {
            SizeViewModel root = this.tree.Items[0] as SizeViewModel;
            root.IsChecked = false;
        }

        
    }
}
