using System;
using System.Diagnostics;
using System.Windows.Controls;

namespace WikiFlight
{
    public class LogTraceListener : TraceListener
    {
        private readonly TextBox textBox;

        public LogTraceListener(TextBox textBox)
        {
            this.textBox = textBox;
        }

        public override void Write(string? message)
        {
            textBox.Text = string.Format("{0}{1}: {2}", textBox.Text, DateTime.Now, message);
            textBox.ScrollToEnd();
        }
        public override void WriteLine(string? message)
        {
            textBox.Text = string.Format("{0}{1}: {2}\n", textBox.Text, DateTime.Now, message);
            textBox.ScrollToEnd();
        }
    }
}
