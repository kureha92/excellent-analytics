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
using Analytics.Data;
using System.Windows.Media.Animation;

namespace UI
{
    /// <summary>
    /// Interaction logic for ExecutionProgress.xaml
    /// </summary>
    public partial class ExecutionProgress : Window
    {
        public ExecutionProgress()
        {
            InitializeComponent();
            this.Show();
        }

        public ExecutionProgress(ReportManager repMan)
        {
            InitializeComponent();
            repMan.executionProgress += new ReportManager.ExecutionProgress(repMan_executionProgress);
            this.Show();
        }

        void repMan_executionProgress(int progress, string progressMessage, string errorMsg)
        {
            if (progress == 100 || !String.IsNullOrEmpty(errorMsg))
            {
                this.Close();
            }
        }
    }
}
