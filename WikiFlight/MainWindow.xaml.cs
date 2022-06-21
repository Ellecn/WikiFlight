using Common;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WikipediaApi;

namespace WikiFlight
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly int REFRESH_INTERVAL_IN_SECONDS = 10;
        private readonly int SEARCH_RADIUS = 5000;
        private readonly string WIKIPEDIA_LANGUAGE_CODE = "de";

        private readonly LogWindow logWindow = new LogWindow();

        private readonly FlightSimulatorClient flightSimulatorClient;
        private readonly WikipediaClient wikipediaClient = new WikipediaClient();
        private readonly WikipediaPageCache wikipediaPageCache = new WikipediaPageCache();
        private readonly DispatcherTimer PositionRefreshTimer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();

            Trace.Listeners.Add(new LogTraceListener(logWindow.txtLog));

            flightSimulatorClient = new FlightSimulatorClient(OnConnected, OnPositionReceived, OnSimExited);

            SetUi();

            PositionRefreshTimer.Interval = TimeSpan.FromSeconds(REFRESH_INTERVAL_IN_SECONDS);
            PositionRefreshTimer.Tick += PositionRefreshTimerTick;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            flightSimulatorClient.Init(this);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Disconnect();
            App.Current.Shutdown();
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

        private void PositionRefreshTimerTick(object sender, EventArgs e)
        {
            flightSimulatorClient.RequestNewPosition();
        }

        private void Connect()
        {
            try
            {
                flightSimulatorClient.Connect();
                flightSimulatorClient.RequestNewPosition();
            }
            catch (Exception exception)
            {
                Trace.WriteLine(string.Format("Could not connect to simulator ({0})", exception.Message));
            }
        }

        private void Disconnect()
        {
            PositionRefreshTimer.Stop();
            flightSimulatorClient.Disconnect();
            wikipediaClient.PositionOfLastRequest = null;
            lstPages.Items.Clear();
            SetUi();
        }

        private async Task Refresh(Position currentPosition)
        {
            if (wikipediaClient.PositionOfLastRequest == null || currentPosition.GetDistance(wikipediaClient.PositionOfLastRequest) > 10)
            {
                var pagesNearby = await wikipediaClient.GetPagesNearby(WIKIPEDIA_LANGUAGE_CODE, currentPosition, SEARCH_RADIUS);
                wikipediaPageCache.AddNewPagesOnly(pagesNearby);

                var pagesWithoutSummary = wikipediaPageCache.GetPagesWithoutSummary(currentPosition, SEARCH_RADIUS);
                if (pagesWithoutSummary.Count > 0)
                {
                    await wikipediaClient.AddSummary(pagesWithoutSummary, WIKIPEDIA_LANGUAGE_CODE);
                }

                DisplayPages(currentPosition);
            }
        }

        private void DisplayPages(Position currentPosition)
        {
            lstPages.Items.Clear();
            var pagesForDisplay = wikipediaPageCache.Get(currentPosition, SEARCH_RADIUS);
            pagesForDisplay.ForEach(p => lstPages.Items.Add(p));
        }

        private void SetUi()
        {
            bool connected = flightSimulatorClient.IsConnected();

            btnDisconnect.IsEnabled = connected;
            btnConnect.IsEnabled = !connected;
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
