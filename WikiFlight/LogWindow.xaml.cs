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

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
