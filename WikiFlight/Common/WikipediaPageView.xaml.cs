using System.Windows.Controls;
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
                    txtTitle.Text = _page.Title;
                    txtDistance.Text = string.Format("({0}m away)", _page.Distance);
                    txtSummary.Text = _page.Summary;
                }
                else
                {
                    txtTitle.Text = string.Empty;
                    txtDistance.Text = string.Empty;
                    txtSummary.Text = string.Empty;
                }
            }
        }

        public void RefreshUi()
        {
            if (_page != null)
            {
                txtDistance.Text = string.Format("({0}m away)", _page.Distance);
            }
        }
    }
}
