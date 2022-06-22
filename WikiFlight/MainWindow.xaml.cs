using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;
using WikiFlight.Common;
using WikiFlight.MSFS2020;
using WikiFlight.Wikipedia;

namespace WikiFlight
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly int REFRESH_INTERVAL_IN_SECONDS = 10;

        private readonly LogWindow logWindow = new LogWindow();

        private readonly FlightSimulatorService flightSimulatorService; // TODO: umbenennen
        private readonly WikipediaService wikipediaService = new WikipediaService(); // TODO: umbenennen

        private readonly DispatcherTimer PositionRefreshTimer = new DispatcherTimer();

        private Position? positionOfLastRequest;

        private readonly Settings settings = new Settings();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = settings;

            Trace.Listeners.Add(new LogTraceListener(logWindow.txtLog));

            flightSimulatorService = new FlightSimulatorService(OnConnected, OnPositionReceived, OnSimExited);

            SetUi();

            PositionRefreshTimer.Interval = TimeSpan.FromSeconds(REFRESH_INTERVAL_IN_SECONDS);
            PositionRefreshTimer.Tick += PositionRefreshTimerTick;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            flightSimulatorService.Init(this);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Disconnect();
            Application.Current.Shutdown();
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            Connect();
        }

        private void btnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            Disconnect();
        }

        private void lstPages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstPages.SelectedItem is WikipediaPage selectedPage)
            {
                lstPages.SelectedIndex = -1;
                Process.Start("explorer", string.Format("\"{0}\"", selectedPage.URL));
            }
        }

        private void btnOpenLog_Click(object sender, RoutedEventArgs e)
        {
            logWindow.Show();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start("explorer", string.Format("\"{0}\"", e.Uri.AbsoluteUri));
        }

        private async void ListViewExpander_Expanded(object sender, RoutedEventArgs e)
        {
            Expander expander = (Expander)sender;
            WikipediaPage wikipediaPage = (WikipediaPage)expander.DataContext;
            if (string.IsNullOrEmpty(wikipediaPage.Summary))
            {
                string? summary = await wikipediaService.GetSummary(wikipediaPage);
                wikipediaPage.Summary = summary ?? "";
            }
        }

        private void CheckBoxAlwaysOnTop_Changed(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            Topmost = checkBox.IsChecked ?? false;
        }

        private void PositionRefreshTimerTick(object sender, EventArgs e)
        {
            flightSimulatorService.RequestNewPosition();
        }

        private void Connect()
        {
            try
            {
                flightSimulatorService.Connect();
                flightSimulatorService.RequestNewPosition();
            }
            catch (Exception exception)
            {
                Trace.WriteLine(string.Format("Could not connect to simulator ({0})", exception.Message));
            }
        }

        private void Disconnect()
        {
            PositionRefreshTimer.Stop();
            flightSimulatorService.Disconnect();
            positionOfLastRequest = null;
            lstPages.Items.Clear();
            SetUi();
        }

        private async Task Refresh(Position currentPosition)
        {
            if (positionOfLastRequest == null || currentPosition.GetDistance(positionOfLastRequest) > 10)
            {
                var pagesNearby = await wikipediaService.GetPagesNearby(settings.WikipediaLanguageCode, currentPosition, settings.SearchRadiusInMeter);
                positionOfLastRequest = currentPosition;

                lstPages.Items.Clear();
                pagesNearby.ForEach(p => lstPages.Items.Add(p));
            }
        }

        private void SetUi()
        {
            bool connected = flightSimulatorService.IsConnected();

            btnDisconnect.IsEnabled = connected;
            btnConnect.IsEnabled = !connected;
            cmbLanguageCode.IsEnabled = !connected;
            cmbRadius.IsEnabled = !connected;
            if (!connected)
            {
                txtLatitude.Text = "n/a";
                txtLongitude.Text = "n/a";
            }
        }

        #region FlightSimulatorClient event handlers

        private void OnConnected()
        {
            PositionRefreshTimer.Start();
            SetUi();
        }

        private async void OnPositionReceived(Position currentPosition)
        {
            txtLatitude.Text = currentPosition.Latitude.ToString();
            txtLongitude.Text = currentPosition.Longitude.ToString();

            await Refresh(currentPosition);
        }

        private void OnSimExited()
        {
            Disconnect();
        }

        #endregion
    }
}
