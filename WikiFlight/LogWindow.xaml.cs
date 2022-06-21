using System;
using System.ComponentModel;
using System.Windows;

namespace WikiFlight
{
    /// <summary>
    /// Interaktionslogik für LogWindow.xaml
    /// </summary>
    public partial class LogWindow : Window
    {
        public LogWindow()
        {
            InitializeComponent();
        }

        protected override void OnContentRendered(EventArgs e)
        {
            txtLog.ScrollToEnd();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(txtLog.Text);
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtLog.Clear();
        }
    }
}
