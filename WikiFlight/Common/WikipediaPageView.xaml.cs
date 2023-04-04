using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using WikiFlight.Wikipedia;

namespace WikiFlight.Common
{
    /// <summary>
    /// Interaction logic for WikipediaPageView.xaml
    /// </summary>
    public partial class WikipediaPageView : UserControl
    {
        public WikipediaPageView()
        {
            InitializeComponent();
        }

        private WikipediaPage? _page;
        public WikipediaPage? Page
        {
            get
            {
                return _page;
            }

            set
            {
                _page = value;
                if (_page != null)
                {
                    txtTitle.Inlines.Clear();
                    Hyperlink hyperlink = new Hyperlink();
                    hyperlink.NavigateUri = new Uri(_page.URL);
                    hyperlink.Inlines.Add(_page.Title);
                    hyperlink.RequestNavigate += Hyperlink_RequestNavigate;
                    txtTitle.Inlines.Add(hyperlink);

                    txtDistance.Text = string.Format("Distance: {0}m", _page.Distance);
                    txtSummary.Text = _page.Summary;
                }
                else
                {
                    txtTitle.Inlines.Clear();
                    txtDistance.Text = string.Empty;
                    txtSummary.Text = string.Empty;
                }
            }
        }

        public void RefreshUi()
        {
            if (_page != null)
            {
                txtDistance.Text = string.Format("Distance: {0}m", _page.Distance);
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            if (_page != null)
            {
                Process.Start("explorer", string.Format("\"{0}\"", _page.URL));
            }
        }
    }
}
